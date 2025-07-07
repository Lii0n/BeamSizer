using BeamSizing; // This is where your BeamCalculator class is

namespace BeamCalculator.Services
{
    public interface IBeamCalculationService
    {
        BeamSizingResults PerformAnalysis(BeamSizerConfig config);
        bool ValidateConfiguration(BeamSizerConfig config);
        List<BeamProperties> GetTopBeams(double ecl, double span, bool capped, int count = 5);
    }

    public class BeamCalculationService : IBeamCalculationService
    {
        public BeamSizingResults PerformAnalysis(BeamSizerConfig config)
        {
            // Fixed: Use the correct namespace - it's BeamSizing.BeamCalculator
            return BeamSizing.BeamCalculator.PerformFullAnalysis(config);
        }

        public bool ValidateConfiguration(BeamSizerConfig config)
        {
            // Fixed: Use the correct namespace - it's BeamSizing.BeamCalculator
            return BeamSizing.BeamCalculator.ValidateConfiguration(config);
        }

        public List<BeamProperties> GetTopBeams(double ecl, double span, bool capped, int count = 5)
        {
            return DataLoader.FindTopAdequateBeams(ecl, span, capped, count);
        }
    }
}