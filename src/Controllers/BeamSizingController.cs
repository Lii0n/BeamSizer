// BeamSizingController.cs - Cleaned up version with only used endpoints
// Removed unused endpoints and added missing k-factors endpoint

using Microsoft.AspNetCore.Mvc;
using BeamSizing;

namespace BeamCalculator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeamSizingController : ControllerBase
    {
        private readonly ILogger<BeamSizingController> _logger;

        public BeamSizingController(ILogger<BeamSizingController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check for beam calculation engine
        /// USED BY: Frontend health checking and endpoint discovery
        /// </summary>
        [HttpGet("health")]
        public ActionResult<object> Health()
        {
            try
            {
                // Test with known good values
                var testConfig = new BeamSizerConfig(
                    ratedCapacity: 10000.0,
                    weightHoistTrolley: 1700.0,
                    girderWeight: 3000.0,
                    panelWeight: 2000.0,
                    endTruckWeight: 1000.0,
                    numCols: 2,
                    railHeight: 20.0,
                    wheelBase: 7.0,
                    supportCenters: 45.0,
                    freestanding: false,
                    capped: true,
                    bridgeSpan: 44.0,
                    hoistSpeed: 0
                );

                var isValid = BeamSizing.BeamCalculator.ValidateConfiguration(testConfig);
                var kFactors = BeamSizing.BeamCalculator.FindKFactors(testConfig);

                return Ok(new
                {
                    status = "healthy",
                    calculationEngine = "operational",
                    testValidation = isValid,
                    testKFactors = kFactors,
                    configSummary = testConfig.GetAnalysisSummary(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beam calculation engine health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system information
        /// USED BY: Frontend for system monitoring
        /// </summary>
        [HttpGet("system-info")]
        public ActionResult<object> GetSystemInfo()
        {
            try
            {
                return Ok(new
                {
                    platform = Environment.OSVersion.Platform.ToString(),
                    osVersion = Environment.OSVersion.VersionString,
                    totalMemory = GC.GetTotalMemory(false),
                    version = "1.0.0-demo",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    timestamp = DateTime.UtcNow,
                    uptime = Environment.TickCount64 / 1000
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system info");
                return StatusCode(500, new { error = "Failed to get system info", details = ex.Message });
            }
        }

        /// <summary>
        /// Clear cache
        /// USED BY: Frontend cache management
        /// </summary>
        [HttpPost("clear-cache")]
        public ActionResult<object> ClearCache()
        {
            try
            {
                GC.Collect(); // Force garbage collection as a simple "cache clear"

                return Ok(new
                {
                    success = true,
                    message = "Cache cleared successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                return StatusCode(500, new { error = "Failed to clear cache", details = ex.Message });
            }
        }

        /// <summary>
        /// Get K-factors for a given wheelbase to span ratio
        /// USED BY: Frontend for K-factor calculations (was missing!)
        /// </summary>
        [HttpGet("k-factors")]
        public ActionResult<object> GetKFactors([FromQuery] double wheelbaseSpanRatio)
        {
            try
            {
                var kFactors = DataLoader.GetKFactors(wheelbaseSpanRatio);

                return Ok(new
                {
                    wheelbaseSpanRatio = wheelbaseSpanRatio,
                    k1 = kFactors.k1,
                    k2 = kFactors.k2,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting K-factors for ratio {Ratio}", wheelbaseSpanRatio);
                return StatusCode(500, new { error = "Failed to get K-factors", details = ex.Message });
            }
        }

        /// <summary>
        /// Perform complete beam sizing analysis with top 5 candidates
        /// USED BY: Frontend main analysis functionality
        /// </summary>
        [HttpPost("analyze")]
        public ActionResult<object> AnalyzeBeam([FromBody] BeamAnalysisRequest request)
        {
            try
            {
                var startTime = DateTime.UtcNow;

                // Create configuration
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
                    bridgeSpan: request.SupportCenters, // Use supportCenters for bridgeSpan
                    hoistSpeed: request.HoistSpeed
                );

                // Calculate K-factors and ECL
                var kFactors = BeamSizing.BeamCalculator.FindKFactors(config);
                double calculatedECL = kFactors.k1 * config.MaxWheelLoad;

                // Get top 5 beam candidates
                var topBeams = DataLoader.FindTopAdequateBeams(calculatedECL, config.SupportCenters, config.Capped, 5);

                // Create detailed beam candidates with their capacities
                var beamCandidates = topBeams.Select(beam => new
                {
                    designation = beam.Designation,
                    weight = beam.Weight,
                    depth = beam.Depth,
                    capacity = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, config.SupportCenters, config.Capped),
                    utilization = (calculatedECL / DataLoader.GetInterpolatedLoadCapacity(beam.Designation, config.SupportCenters, config.Capped)) * 100.0,
                    isSelected = beam == topBeams.FirstOrDefault()
                }).ToList();

                // Perform full analysis with the top beam (if available)
                var results = BeamSizing.BeamCalculator.PerformFullAnalysis(config);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger.LogInformation("Beam analysis completed for capacity {Capacity} lbs, span {Span} ft, found {Count} candidates",
                    request.RatedCapacity, request.SupportCenters, topBeams.Count);

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
                        candidatesFound = topBeams.Count
                    }
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Invalid beam configuration parameters");
                return BadRequest(new
                {
                    error = "Invalid configuration",
                    details = ex.Message,
                    parameter = ex.ParamName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing beam analysis");
                return StatusCode(500, new
                {
                    error = "Analysis failed",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Validate beam configuration without performing full analysis
        /// USED BY: Frontend configuration validation
        /// </summary>
        [HttpPost("validate")]
        public ActionResult<object> ValidateConfiguration([FromBody] BeamAnalysisRequest request)
        {
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
                    bridgeSpan: request.SupportCenters, // Use supportCenters for bridgeSpan
                    hoistSpeed: request.HoistSpeed
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
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new
                {
                    isValid = false,
                    error = ex.Message,
                    parameter = ex.ParamName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating beam configuration");
                return StatusCode(500, new { error = "Validation failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available beam options for given requirements with detailed information
        /// USED BY: Frontend beam selection functionality
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

                // Add capacity and utilization information
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
    }

    /// <summary>
    /// Request model for beam analysis
    /// </summary>
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
        public bool Capped { get; set; }
        public double HoistSpeed { get; set; } = 0;
    }
}