// /src/Services/Interfaces/IAnalysisService.cs
using BeamCalculator.Data.Models;

namespace BeamCalculator.Services
{
    public interface IAnalysisService
    {
        Task<SavedAnalysis> SaveAnalysisAsync(string projectName, object config, object results, int userId, string notes = "");
        Task<SavedAnalysis?> GetAnalysisAsync(int id, int userId);
        Task<List<SavedAnalysis>> GetUserAnalysesAsync(int userId);
        Task<bool> DeleteAnalysisAsync(int id, int userId);
    }
}