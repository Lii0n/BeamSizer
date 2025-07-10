// BeamCalculator.cs - Refactored version with duplicate calculations removed
// All user-input-dependent calculations moved to BeamSizerConfig

using System;
using System.Collections.Generic;
using System.Linq;

namespace BeamSizing
{
    public static class BeamCalculator
    {
        /// <summary>
        /// Find K-factors using the wheelbase to support centers ratio.
        /// </summary>
        public static (double k1, double k2) FindKFactors(BeamSizerConfig config)
        {
            return DataLoader.GetKFactors(config.WheelbaseSpanRatio);
        }

        /// <summary>
        /// Find the lightest adequate beam for the given configuration.
        /// Returns both the selected beam and top candidates to avoid duplicate searches
        /// </summary>
        public static (BeamProperties selectedBeam, List<BeamProperties> topCandidates) FindBeamSizeWithCandidates(BeamSizerConfig config, double k1)
        {
            double ECL = k1 * config.MaxWheelLoad;
            double bridgeSpan = config.BridgeSpan;

            Console.WriteLine($"DEBUG: FindBeamSizeWithCandidates using BridgeSpan = {bridgeSpan:F1} ft, ECL = {ECL:F0} lbs");

            // Get top candidates - this is the SINGLE call that does all the work
            var topBeams = DataLoader.FindTopAdequateBeams(ECL, bridgeSpan, config.Capped, 5);

            var selectedBeam = topBeams.FirstOrDefault();

            if (selectedBeam == null)
            {
                throw new InvalidOperationException(
                    $"No adequate beam found for ECL={ECL:F0} lbs and span={bridgeSpan:F1} ft. " +
                    $"Consider using a {(config.Capped ? "larger" : "capped")} beam system or reducing loads.");
            }

            Console.WriteLine($"DEBUG: Selected beam {selectedBeam.Designation} with weight {selectedBeam.Weight:F1} lbs/ft");

            // Display the candidates
            Console.WriteLine("\nTop Beam Candidates (sorted by weight only):");
            foreach (var beam in topBeams)
            {
                double beamCapacity = DataLoader.GetInterpolatedLoadCapacity(beam.Designation, bridgeSpan, config.Capped);
                double utilizationPercent = (ECL / beamCapacity) * 100;

                // Get table values for engineering validation
                var tableInfo = GetBeamTableValidation(beam.Designation, bridgeSpan, config.Capped);

                Console.WriteLine($"  • {beam.Designation} — {beam.Weight:F1} lbs/ft, I = {beam.I:F0} in⁴, S = {beam.S:F0} in³");
                Console.WriteLine($"    Table Data: {tableInfo}");
                Console.WriteLine($"    Final Capacity = {beamCapacity:N0} lbs, Utilization = {utilizationPercent:F1}%");
                Console.WriteLine();
            }

            return (selectedBeam, topBeams);
        }

        /// <summary>
        /// Check lateral deflection limit (L/450).
        /// Uses pre-calculated values from config where possible.
        /// </summary>
        public static bool CheckLateralDeflection(BeamSizerConfig config, BeamProperties beam)
        {
            if (beam == null) return false;

            double beamDeflection = (config.LateralLoad * Math.Pow(config.RailHeightInches, 3)) /
                                   (3.0 * 29000000.0 * beam.I);
            double allowableDeflection = config.RailHeightInches / 450.0;

            return beamDeflection < allowableDeflection;
        }

        /// <summary>
        /// Check longitudinal deflection limit (L/500).
        /// Uses pre-calculated values from config where possible.
        /// </summary>
        public static bool CheckLongitudinalDeflection(BeamSizerConfig config, BeamProperties beam)
        {
            if (beam == null) return false;

            double beamDeflection = (config.LongitudinalLoad * Math.Pow(config.RailHeightInches, 3)) /
                                   (3.0 * 29000000.0 * beam.I);
            double allowableDeflection = config.RailHeightInches / 500.0;

            return beamDeflection < allowableDeflection;
        }

        /// <summary>
        /// Check bending stress limit (24,000 psi).
        /// Uses pre-calculated values from config where possible.
        /// </summary>
        public static bool CheckBendingStress(BeamSizerConfig config, BeamProperties beam)
        {
            if (beam == null) return false;

            double beamStress = (config.LateralLoad * config.RailHeightInches) / beam.S;
            double allowableStress = 24000;

            return beamStress < allowableStress;
        }

        /// <summary>
        /// Check axial unity check.
        /// Uses pre-calculated values from config where possible.
        /// </summary>
        public static bool CheckAxialUnity(BeamSizerConfig config, double axialLoad)
        {
            // Unity check: fa/Fa + fe/Fe < 1.0
            double unityRatio = (axialLoad / 24000.0) + (config.EffectiveLength / 43.2);
            Console.WriteLine($"DEBUG: Unity ratio is {unityRatio:F3}");
            return unityRatio < 1.0;
        }

        /// <summary>
        /// Perform complete beam sizing analysis using pure functions.
        /// MAIN ENTRY POINT - Called by BeamSizerService.PerformAnalysis()
        /// Now uses pre-calculated values from config to eliminate duplication.
        /// </summary>
        public static BeamSizingResults PerformFullAnalysis(BeamSizerConfig config)
        {
            var results = new BeamSizingResults();

            try
            {
                // Step 1: Find K-factors
                var kFactors = FindKFactors(config);
                results.K1 = kFactors.k1;
                results.K2 = kFactors.k2;

                // Store configuration values in results
                results.WheelbaseSpanRatio = config.WheelbaseSpanRatio;
                results.ImpactFactor = config.ImpactFactor;

                // Store beam weight components
                results.GirderWeight = config.GirderWeight;
                results.PanelWeight = config.PanelWeight;
                results.EndTruckWeight = config.EndTruckWeight;
                results.TotalBeamWeight = config.WeightBeam;

                // Step 2: Single call gets both selected beam and candidates
                Console.WriteLine($"DEBUG: bridgeSpan in config is {config.BridgeSpan:F1} ft");

                var (selectedBeam, topCandidates) = FindBeamSizeWithCandidates(config, kFactors.k1);
                results.SelectedBeam = selectedBeam;
                results.TopBeamCandidates = topCandidates;

                // Step 3: Calculate loads (using pre-calculated values from config)
                results.MaxWheelLoad = config.MaxWheelLoad;
                results.RunwayBeamWeight = config.CalculateRunwayBeamWeight(selectedBeam);
                results.LateralLoad = config.LateralLoad;
                results.LongitudinalLoad = config.LongitudinalLoad;

                // Step 4: Calculate moments (using config methods)
                results.ColumnMoment = config.CalculateColumnMoment();
                results.FoundationMoment = config.CalculateFoundationMoment();
                results.LateralOTM = BeamSizerConfig.ConvertToOTM(results.ColumnMoment);
                results.LongitudinalOTM = BeamSizerConfig.ConvertToOTM(results.FoundationMoment);

                // Step 5: Calculate foundation loads (using config methods)
                results.MaxVerticalLoad = config.CalculateMaxVerticalLoad(results.RunwayBeamWeight);
                results.ColumnLoadFoundation = BeamSizerConfig.CalculateColumnLoadFoundation(results.MaxVerticalLoad);

                // Step 6: Perform structural checks (simplified with pre-calculated values)
                results.LateralDeflectionPass = CheckLateralDeflection(config, selectedBeam);
                results.LongitudinalDeflectionPass = CheckLongitudinalDeflection(config, selectedBeam);
                results.StressCheckPass = CheckBendingStress(config, selectedBeam);
                results.AxialCheckPass = CheckAxialUnity(config, results.MaxVerticalLoad);

                // Step 7: Set overall result
                results.OverallPass = results.LateralDeflectionPass &&
                                     results.LongitudinalDeflectionPass &&
                                     results.StressCheckPass &&
                                     results.AxialCheckPass;

                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Analysis failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates a configuration without performing full analysis.
        /// USED BY: BeamSizingController.ValidateConfiguration() and Health checks
        /// </summary>
        public static bool ValidateConfiguration(BeamSizerConfig config)
        {
            try
            {
                // Configuration validation occurs during struct construction
                // If we can access properties, validation passed
                _ = config.MaxWheelLoad;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get beam table validation information for engineering review
        /// Shows exactly what table values are being used in calculations
        /// </summary>
        private static string GetBeamTableValidation(string beamDesignation, double span, bool capped)
        {
            var lookup = capped ? DataLoader.CappedCapacities : DataLoader.UncappedCapacities;

            // Check if we have exact span match
            int exactSpan = (int)Math.Round(span);
            bool hasExactMatch = lookup.ContainsKey((beamDesignation, exactSpan));

            if (hasExactMatch && Math.Abs(span - exactSpan) < 0.001)
            {
                // Exact match case
                int exactCapacity = lookup[(beamDesignation, exactSpan)];
                return $"Exact match at {exactSpan} ft = {exactCapacity:N0} lbs";
            }
            else
            {
                // Interpolation case - show the two points being interpolated
                var beamSpans = lookup.Keys
                    .Where(k => k.beam == beamDesignation)
                    .Select(k => k.span)
                    .OrderBy(s => s)
                    .ToList();

                if (beamSpans.Count == 0)
                {
                    return "ERROR: No table data found";
                }

                var lowerSpan = beamSpans.Where(s => s <= span).DefaultIfEmpty(0).Max();
                var upperSpan = beamSpans.Where(s => s >= span).DefaultIfEmpty(0).Min();

                if (lowerSpan == 0 || upperSpan == 0)
                {
                    return "ERROR: Span outside table range";
                }

                if (lowerSpan == upperSpan)
                {
                    // Exact match after all
                    int capacity = lookup[(beamDesignation, lowerSpan)];
                    return $"Table match at {lowerSpan} ft = {capacity:N0} lbs";
                }
                else
                {
                    // Show interpolation points
                    int lowerCapacity = lookup[(beamDesignation, lowerSpan)];
                    int upperCapacity = lookup[(beamDesignation, upperSpan)];
                    double interpolationFactor = (span - lowerSpan) / (upperSpan - lowerSpan);

                    return $"Interpolated between: {lowerSpan}ft({lowerCapacity:N0}) ↔ {upperSpan}ft({upperCapacity:N0}), factor={interpolationFactor:F3}";
                }
            }
        }
    }
}