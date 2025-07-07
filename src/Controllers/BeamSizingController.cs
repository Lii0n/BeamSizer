using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeamSizing;
using BeamCalculator.Data.Models;
using System.Text.Json;

namespace BeamCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeamSizingController : ControllerBase
    {
        private readonly ILogger<BeamSizingController> _logger;
        private readonly ApplicationDbContext _context;

        public BeamSizingController(ILogger<BeamSizingController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Health check for beam calculation engine
        /// </summary>
        [HttpGet("health")]
        public ActionResult<object> Health()
        {
            try
            {
                var testConfig = new BeamSizerConfig(10000.0, 1700.0, 3000.0, 2000.0, 1000.0, 2, 20.0, 7.0, 45.0, false, true, 44.0, 0);
                var isValid = BeamSizing.BeamCalculator.ValidateConfiguration(testConfig);
                var kFactors = BeamSizing.BeamCalculator.FindKFactors(testConfig);

                return Ok(new
                {
                    status = "healthy",
                    calculationEngine = "operational",
                    testValidation = isValid,
                    testKFactors = kFactors,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system information
        /// </summary>
        [HttpGet("system-info")]
        public async Task<ActionResult<object>> GetSystemInfo()
        {
            try
            {
                var savedCount = await _context.SavedAnalyses.CountAsync();

                return Ok(new
                {
                    platform = Environment.OSVersion.Platform.ToString(),
                    osVersion = Environment.OSVersion.VersionString,
                    totalMemory = GC.GetTotalMemory(false),
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    timestamp = DateTime.UtcNow,
                    uptime = Environment.TickCount64 / 1000,
                    totalSavedAnalyses = savedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System info failed");
                return StatusCode(500, new { error = "Failed to get system info", details = ex.Message });
            }
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        [HttpPost("clear-cache")]
        public ActionResult<object> ClearCache()
        {
            try
            {
                GC.Collect();
                return Ok(new
                {
                    success = true,
                    message = "Cache cleared successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache clear failed");
                return StatusCode(500, new { error = "Failed to clear cache", details = ex.Message });
            }
        }

        /// <summary>
        /// Validate beam configuration
        /// </summary>
        [HttpPost("validate")]
        public ActionResult<object> ValidateConfiguration([FromBody] BeamAnalysisRequest request)
        {
            try
            {
                var config = new BeamSizerConfig(
                    request.RatedCapacity, request.WeightHoistTrolley, request.GirderWeight,
                    request.PanelWeight, request.EndTruckWeight, request.NumCols,
                    request.RailHeight, request.WheelBase, request.SupportCenters,
                    request.Freestanding, request.Capped, request.SupportCenters, request.HoistSpeed
                );

                var isValid = BeamSizing.BeamCalculator.ValidateConfiguration(config);

                return Ok(new
                {
                    isValid = isValid,
                    configSummary = config.GetAnalysisSummary(),
                    calculatedValues = new
                    {
                        totalBeamWeight = config.WeightBeam,
                        maxWheelLoad = config.MaxWheelLoad,
                        impactFactor = config.ImpactFactor,
                        wheelbaseSpanRatio = config.WheelbaseSpanRatio
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation failed");
                return BadRequest(new { isValid = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Perform beam analysis
        /// </summary>
        [HttpPost("analyze")]
        public ActionResult<object> AnalyzeBeam([FromBody] BeamAnalysisRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var config = new BeamSizerConfig(
                    ratedCapacity: request.RatedCapacity,
                    weightHoistTrolley: request.WeightHoistTrolley,
                    girderWeight: request.GirderWeight,
                    panelWeight: request.PanelWeight,
                    endTruckWeight: request.EndTruckWeight,
                    numCols: request.NumCols,
                    railHeight: request.RailHeight,
                    wheelBase: request.WheelBase,
                    supportCenters: request.SupportCenters,
                    freestanding: request.Freestanding,
                    capped: request.Capped,
                    bridgeSpan: request.SupportCenters,
                    hoistSpeed: request.HoistSpeed
                );

                // Validate configuration
                var isValid = BeamSizing.BeamCalculator.ValidateConfiguration(config);
                if (!isValid)
                {
                    return BadRequest(new { error = "Invalid configuration parameters" });
                }

                // Calculate K-factors and ECL
                var kFactors = BeamSizing.BeamCalculator.FindKFactors(config);
                var calculatedECL = kFactors.k1 * config.MaxWheelLoad;

                // Get beam candidates
                var topBeams = DataLoader.FindTopAdequateBeams(calculatedECL, config.SupportCenters, config.Capped, 10);
                var beamCandidates = topBeams.Take(5).Select((beam, index) => new
                {
                    designation = beam.Designation,
                    weight = beam.Weight,
                    depth = beam.Depth,
                    capacity = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, config.SupportCenters, config.Capped),
                    utilization = (calculatedECL / DataLoader.GetInterpolatedLoadCapacity(beam.Designation, config.SupportCenters, config.Capped)) * 100.0,
                    rank = index + 1,
                    isRecommended = index == 0,
                    marginOfSafety = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, config.SupportCenters, config.Capped) - calculatedECL
                }).ToList();

                // Perform full analysis
                var results = BeamSizing.BeamCalculator.PerformFullAnalysis(config);

                stopwatch.Stop();
                var processingTime = stopwatch.Elapsed.TotalMilliseconds;

                return Ok(new
                {
                    results = results,
                    calculatedECL = calculatedECL,
                    kFactors = new { k1 = kFactors.k1, k2 = kFactors.k2 },
                    beamCandidates = beamCandidates,
                    metadata = new
                    {
                        processingTimeMs = processingTime,
                        cached = false,
                        timestamp = DateTime.UtcNow,
                        candidatesFound = beamCandidates.Count
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error performing beam analysis for {Capacity}lb capacity", request.RatedCapacity);
                return StatusCode(500, new { error = "Analysis failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get beam options for given requirements
        /// </summary>
        [HttpGet("beams")]
        public ActionResult<object> GetBeamOptions(
            [FromQuery] double ecl,
            [FromQuery] double span,
            [FromQuery] bool capped = false,
            [FromQuery] int limit = 5)
        {
            try
            {
                var beams = DataLoader.FindTopAdequateBeams(ecl, span, capped, limit);

                var detailedBeams = beams.Select(beam => new
                {
                    designation = beam.Designation,
                    weight = beam.Weight,
                    depth = beam.Depth,
                    capacity = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, span, capped),
                    utilization = (ecl / DataLoader.GetInterpolatedLoadCapacity(beam.Designation, span, capped)) * 100.0,
                    margin = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, span, capped) - ecl
                }).ToList();

                return Ok(new
                {
                    requiredECL = ecl,
                    span = span,
                    capped = capped,
                    beams = detailedBeams,
                    count = detailedBeams.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting beam options for ECL={ECL}, span={Span}", ecl, span);
                return StatusCode(500, new { error = "Failed to get beam options", details = ex.Message });
            }
        }

        // BASIC SAVE/LOAD WITHOUT SERVICE DEPENDENCY (direct database access)

        /// <summary>
        /// Save current analysis (basic version)
        /// </summary>
        [HttpPost("save-analysis")]
        public async Task<ActionResult<object>> SaveAnalysis([FromBody] SaveAnalysisRequest request)
        {
            try
            {
                var analysis = new SavedAnalysis
                {
                    ProjectName = request.ProjectName,
                    UserId = request.UserId ?? 1,
                    ConfigurationJson = JsonSerializer.Serialize(request.Configuration),
                    AnalysisResultsJson = JsonSerializer.Serialize(request.Results ?? new { }),
                    Notes = request.Notes ?? "",
                    CreatedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                _context.SavedAnalyses.Add(analysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Analysis saved: {ProjectName} with ID {AnalysisId}", request.ProjectName, analysis.Id);

                return Ok(new
                {
                    success = true,
                    analysisId = analysis.Id,
                    projectName = analysis.ProjectName,
                    savedAt = analysis.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving analysis");
                return StatusCode(500, new { error = "Failed to save analysis", details = ex.Message });
            }
        }

        /// <summary>
        /// Load saved analysis by ID (basic version)
        /// </summary>
        [HttpGet("load-analysis/{id}")]
        public async Task<ActionResult<object>> LoadAnalysis(int id, [FromQuery] int userId = 1)
        {
            try
            {
                var analysis = await _context.SavedAnalyses
                    .Where(a => a.Id == id && a.UserId == userId)
                    .FirstOrDefaultAsync();

                if (analysis == null)
                {
                    return NotFound(new { error = "Analysis not found" });
                }

                return Ok(new
                {
                    id = analysis.Id,
                    projectName = analysis.ProjectName,
                    configuration = analysis.ConfigurationJson,
                    results = analysis.AnalysisResultsJson,
                    notes = analysis.Notes,
                    createdDate = analysis.CreatedDate,
                    lastModified = analysis.LastModified
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analysis {Id}", id);
                return StatusCode(500, new { error = "Failed to load analysis", details = ex.Message });
            }
        }

        /// <summary>
        /// Get list of saved analyses (basic version)
        /// </summary>
        [HttpGet("saved-analyses")]
        public async Task<ActionResult<object>> GetSavedAnalyses([FromQuery] int userId = 1)
        {
            try
            {
                var analyses = await _context.SavedAnalyses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedDate)
                    .Select(a => new
                    {
                        a.Id,
                        a.ProjectName,
                        a.CreatedDate,
                        a.Notes
                    })
                    .ToListAsync();

                return Ok(new
                {
                    count = analyses.Count,
                    analyses = analyses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved analyses");
                return StatusCode(500, new { error = "Failed to retrieve analyses", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete saved analysis (basic version)
        /// </summary>
        [HttpDelete("delete-analysis/{id}")]
        public async Task<ActionResult<object>> DeleteAnalysis(int id, [FromQuery] int userId = 1)
        {
            try
            {
                var analysis = await _context.SavedAnalyses
                    .Where(a => a.Id == id && a.UserId == userId)
                    .FirstOrDefaultAsync();

                if (analysis == null)
                {
                    return NotFound(new { error = "Analysis not found" });
                }

                _context.SavedAnalyses.Remove(analysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Analysis deleted: ID {AnalysisId}", id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting analysis {Id}", id);
                return StatusCode(500, new { error = "Failed to delete analysis", details = ex.Message });
            }
        }
    }

    // REQUEST MODELS
    public class BeamAnalysisRequest
    {
        public double RatedCapacity { get; set; }
        public double WeightHoistTrolley { get; set; }
        public double GirderWeight { get; set; }
        public double PanelWeight { get; set; }
        public double EndTruckWeight { get; set; }
        public int NumCols { get; set; } = 2;
        public double RailHeight { get; set; }
        public double WheelBase { get; set; }
        public double SupportCenters { get; set; }
        public bool Freestanding { get; set; }
        public bool Capped { get; set; } = true;
        public double HoistSpeed { get; set; } = 0;
    }

    public class SaveAnalysisRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public object Configuration { get; set; } = new { };
        public object? Results { get; set; }
        public string? Notes { get; set; }
        public int? UserId { get; set; } = 1;
    }
}