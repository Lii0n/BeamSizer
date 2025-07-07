using BeamCalculator.Data;
using BeamCalculator.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BeamCalculator.Services
{
    public interface IAnalysisService
    {
        Task<SavedAnalysis> SaveAnalysisAsync(string projectName, object config, object results, int userId, string notes = "");
        Task<SavedAnalysis?> GetAnalysisAsync(int id, int userId);
        Task<List<SavedAnalysis>> GetUserAnalysesAsync(int userId);
        Task<bool> DeleteAnalysisAsync(int id, int userId);
    }

    public class AnalysisService : IAnalysisService
    {
        private readonly ApplicationDbContext _context;

        public AnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavedAnalysis> SaveAnalysisAsync(string projectName, object config, object results, int userId, string notes = "")
        {
            var analysis = new SavedAnalysis
            {
                ProjectName = projectName,
                UserId = userId,
                ConfigurationJson = System.Text.Json.JsonSerializer.Serialize(config),
                AnalysisResultsJson = System.Text.Json.JsonSerializer.Serialize(results),
                Notes = notes,
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.SavedAnalyses.Add(analysis);
            await _context.SaveChangesAsync();
            return analysis;
        }

        public async Task<SavedAnalysis?> GetAnalysisAsync(int id, int userId)
        {
            return await _context.SavedAnalyses
                .Where(a => a.Id == id && a.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SavedAnalysis>> GetUserAnalysesAsync(int userId)
        {
            return await _context.SavedAnalyses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.LastModified)
                .ToListAsync();
        }

        public async Task<bool> DeleteAnalysisAsync(int id, int userId)
        {
            var analysis = await GetAnalysisAsync(id, userId);
            if (analysis == null) return false;

            _context.SavedAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}