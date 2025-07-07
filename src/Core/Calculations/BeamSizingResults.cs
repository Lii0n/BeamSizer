// BeamSizingResults.cs
// Results container for Beam sizing analysis

using System;
using System.Collections.Generic;

namespace BeamSizing
{
    /// <summary>
    /// Contains all calculated values and check results from Beam sizing analysis
    /// </summary>
    public class BeamSizingResults
    {
        #region K-Factors and Beam Selection
        /// <summary>
        /// Load distribution factor 1 (dimensionless)
        /// </summary>
        public double K1 { get; set; }

        /// <summary>
        /// Load distribution factor 2 (dimensionless)
        /// </summary>
        public double K2 { get; set; }

        /// <summary>
        /// Selected runway beam properties
        /// </summary>
        public BeamProperties? SelectedBeam { get; set; }
        /// <summary>
        /// Top 5 beams for ECL
        /// </summary>
        public List<BeamProperties> TopBeamCandidates { get; set; } = new List<BeamProperties>();


        #endregion

        #region Load Calculations
        /// <summary>
        /// Maximum wheel load from Beam (lbs)
        /// </summary>
        public double MaxWheelLoad { get; set; }

        /// <summary>
        /// Total weight of runway beam (lbs)
        /// </summary>
        public double RunwayBeamWeight { get; set; }

        /// <summary>
        /// Lateral load on columns (lbs)
        /// 20% of (Beam capacity + hoist/trolley weight)
        /// </summary>
        public double LateralLoad { get; set; }

        /// <summary>
        /// Longitudinal load on columns (lbs)
        /// 10% of maximum wheel load
        /// </summary>
        public double LongitudinalLoad { get; set; }
        #endregion

        #region Moment Calculations
        /// <summary>
        /// Column moment from lateral load (lb-in)
        /// </summary>
        public double ColumnMoment { get; set; }

        /// <summary>
        /// Foundation moment from longitudinal load (lb-in)
        /// </summary>
        public double FoundationMoment { get; set; }

        /// <summary>
        /// Lateral overturning moment (kip-ft)
        /// </summary>
        public double LateralOTM { get; set; }

        /// <summary>
        /// Longitudinal overturning moment (kip-ft)
        /// </summary>
        public double LongitudinalOTM { get; set; }
        #endregion

        #region Foundation Loads
        /// <summary>
        /// Maximum vertical load on runway system (lbs)
        /// </summary>
        public double MaxVerticalLoad { get; set; }

        /// <summary>
        /// Column load transmitted to foundation (kips)
        /// </summary>
        public double ColumnLoadFoundation { get; set; }
        #endregion

        #region Structural Check Results
        /// <summary>
        /// Lateral deflection check result (L/450 limit)
        /// </summary>
        public bool LateralDeflectionPass { get; set; }

        /// <summary>
        /// Longitudinal deflection check result (L/500 limit)
        /// </summary>
        public bool LongitudinalDeflectionPass { get; set; }

        /// <summary>
        /// Bending stress check result (24,000 psi limit)
        /// </summary>
        public bool StressCheckPass { get; set; }

        /// <summary>
        /// Axial unity check result (interaction formula)
        /// </summary>
        public bool AxialCheckPass { get; set; }

        /// <summary>
        /// Overall design adequacy (all checks must pass)
        /// </summary>
        public bool OverallPass { get; set; }
        #endregion

        #region Detailed Analysis Values (for debugging/validation)
        /// <summary>
        /// Equivalent Concentrated Load used for beam selection (lbs)
        /// </summary>
        public double ECL => K1 * MaxWheelLoad;

        /// <summary>
        /// Wheelbase to span ratio used for K-factor lookup
        /// </summary>
        public double WheelbaseSpanRatio { get; set; }

        /// <summary>
        /// Impact factor applied to Beam loads
        /// </summary>
        public double ImpactFactor { get; set; }

        /// <summary>
        /// Analysis timestamp
        /// </summary>
        public DateTime AnalysisDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Beam component weights from config
        /// </summary>
        public double GirderWeight { get; set; }
        public double PanelWeight { get; set; }
        public double EndTruckWeight { get; set; }
        public double TotalBeamWeight { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if all structural criteria are satisfied
        /// </summary>
        /// <returns>True if all checks pass</returns>
        public bool AllChecksPass()
        {
            return LateralDeflectionPass &&
                   LongitudinalDeflectionPass &&
                   StressCheckPass &&
                   AxialCheckPass;
        }

        /// <summary>
        /// Gets a summary of failed checks
        /// </summary>
        /// <returns>String describing which checks failed</returns>
        public string GetFailedChecks()
        {
            var failed = new System.Collections.Generic.List<string>();

            if (!LateralDeflectionPass) failed.Add("Lateral Deflection");
            if (!LongitudinalDeflectionPass) failed.Add("Longitudinal Deflection");
            if (!StressCheckPass) failed.Add("Bending Stress");
            if (!AxialCheckPass) failed.Add("Axial Unity");

            return failed.Count == 0 ? "All checks passed" : string.Join(", ", failed);
        }

        /// <summary>
        /// Print formatted results to console
        /// </summary>
        public void PrintResults()
        {
            Console.WriteLine("\n" + "=".PadRight(60, '='));
            Console.WriteLine("Beam SIZING ANALYSIS RESULTS");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine($"Analysis Date: {AnalysisDate:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine("\nBeam WEIGHT COMPONENTS:");
            Console.WriteLine($"  Girder: {GirderWeight:F0} lbs");
            Console.WriteLine($"  Panel: {PanelWeight:F0} lbs");
            Console.WriteLine($"  End Truck: {EndTruckWeight:F0} lbs");
            Console.WriteLine($"  Total Beam Weight: {TotalBeamWeight:F0} lbs");

            Console.WriteLine("\nK-FACTORS AND BEAM SELECTION:");
            Console.WriteLine($"  Wheelbase/Span Ratio: {WheelbaseSpanRatio:F4}");
            Console.WriteLine($"  K1 Factor: {K1:F3}");
            Console.WriteLine($"  K2 Factor: {K2:F3}");
            Console.WriteLine($"  Selected Beam: {SelectedBeam?.Designation ?? "None"}");
            if (SelectedBeam != null)
            {
                Console.WriteLine($"  Beam Weight: {SelectedBeam.Weight:F1} lbs/ft");
                Console.WriteLine($"  Moment of Inertia (I): {SelectedBeam.I:F1} in⁴");
                Console.WriteLine($"  Section Modulus (S): {SelectedBeam.S:F1} in³");
                Console.WriteLine($"  Beam Depth: {SelectedBeam.Depth:F1} in");
            }

            Console.WriteLine("\nLOAD CALCULATIONS:");
            Console.WriteLine($"  Impact Factor: {ImpactFactor:F2}");
            Console.WriteLine($"  Max Wheel Load: {MaxWheelLoad:F0} lbs");
            Console.WriteLine($"  Equivalent Concentrated Load (ECL): {ECL:F0} lbs");
            Console.WriteLine($"  Runway Beam Weight: {RunwayBeamWeight:F0} lbs");
            Console.WriteLine($"  Lateral Load: {LateralLoad:F0} lbs");
            Console.WriteLine($"  Longitudinal Load: {LongitudinalLoad:F0} lbs");

            Console.WriteLine("\nMOMENT CALCULATIONS:");
            Console.WriteLine($"  Column Moment: {ColumnMoment:F0} lb-in");
            Console.WriteLine($"  Foundation Moment: {FoundationMoment:F0} lb-in");
            Console.WriteLine($"  Lateral OTM: {LateralOTM:F1} kip-ft");
            Console.WriteLine($"  Longitudinal OTM: {LongitudinalOTM:F1} kip-ft");

            Console.WriteLine("\nFOUNDATION LOADS:");
            Console.WriteLine($"  Max Vertical Load: {MaxVerticalLoad:F0} lbs");
            Console.WriteLine($"  Column Load on Foundation: {ColumnLoadFoundation:F1} kips");

            Console.WriteLine("\nSTRUCTURAL CHECKS:");
            Console.WriteLine($"  Lateral Deflection (L/450): {GetCheckStatus(LateralDeflectionPass)}");
            Console.WriteLine($"  Longitudinal Deflection (L/500): {GetCheckStatus(LongitudinalDeflectionPass)}");
            Console.WriteLine($"  Bending Stress (24 ksi): {GetCheckStatus(StressCheckPass)}");
            Console.WriteLine($"  Axial Unity Check: {GetCheckStatus(AxialCheckPass)}");

            Console.WriteLine("\nOVERALL RESULT:");
            OverallPass = AllChecksPass();
            Console.WriteLine($"  Design Status: {(OverallPass ? "✓ ACCEPTABLE" : "✗ REQUIRES REVISION")}");

            if (!OverallPass)
            {
                Console.WriteLine($"  Failed Checks: {GetFailedChecks()}");
                Console.WriteLine("\nRECOMMENDATIONS:");
                PrintRecommendations();
            }

            Console.WriteLine("=".PadRight(60, '=') + "\n");
        }

        /// <summary>
        /// Print engineering recommendations for failed checks
        /// </summary>
        private void PrintRecommendations()
        {
            if (!LateralDeflectionPass)
                Console.WriteLine("  • Lateral Deflection: Increase beam depth or moment of inertia");

            if (!LongitudinalDeflectionPass)
                Console.WriteLine("  • Longitudinal Deflection: Increase beam depth or moment of inertia");

            if (!StressCheckPass)
                Console.WriteLine("  • Bending Stress: Increase beam section modulus or reduce lateral loads");

            if (!AxialCheckPass)
                Console.WriteLine("  • Axial Unity: Increase column size, reduce height, or add bracing");

            if (!LateralDeflectionPass || !LongitudinalDeflectionPass || !StressCheckPass)
            {
                Console.WriteLine("  • Consider using a capped beam system for increased capacity");
                Console.WriteLine("  • Verify Beam wheel loads and impact factors");
            }
        }

        /// <summary>
        /// Get formatted status string for checks
        /// </summary>
        /// <param name="passed">Whether the check passed</param>
        /// <returns>Formatted status string</returns>
        private string GetCheckStatus(bool passed)
        {
            return passed ? "PASS ✓" : "FAIL ✗";
        }

        /// <summary>
        /// Export results to CSV format string
        /// </summary>
        /// <returns>CSV formatted string of key results</returns>
        public string ToCsvString()
        {
            return $"{AnalysisDate:yyyy-MM-dd HH:mm:ss}," +
                   $"{SelectedBeam?.Designation ?? "N/A"}," +
                   $"{MaxWheelLoad:F0}," +
                   $"{ECL:F0}," +
                   $"{LateralLoad:F0}," +
                   $"{LongitudinalLoad:F0}," +
                   $"{LateralOTM:F1}," +
                   $"{LongitudinalOTM:F1}," +
                   $"{ColumnLoadFoundation:F1}," +
                   $"{GirderWeight:F0}," +
                   $"{PanelWeight:F0}," +
                   $"{EndTruckWeight:F0}," +
                   $"{TotalBeamWeight:F0}," +
                   $"{LateralDeflectionPass}," +
                   $"{LongitudinalDeflectionPass}," +
                   $"{StressCheckPass}," +
                   $"{AxialCheckPass}," +
                   $"{OverallPass}";
        }

        /// <summary>
        /// Get CSV header string
        /// </summary>
        /// <returns>CSV header for results export</returns>
        public static string GetCsvHeader()
        {
            return "AnalysisDate,BeamDesignation,MaxWheelLoad_lbs,ECL_lbs,LateralLoad_lbs," +
                   "LongitudinalLoad_lbs,LateralOTM_kipft,LongitudinalOTM_kipft," +
                   "ColumnLoad_kips,GirderWeight_lbs,PanelWeight_lbs,EndTruckWeight_lbs,TotalBeamWeight_lbs," +
                   "LatDeflPass,LongDeflPass,StressPass,AxialPass,OverallPass";
        }

        /// <summary>
        /// Create a copy of the results for comparison purposes
        /// </summary>
        /// <returns>Deep copy of the current results</returns>
        public BeamSizingResults Clone()
        {
            return new BeamSizingResults
            {
                K1 = this.K1,
                K2 = this.K2,
                SelectedBeam = this.SelectedBeam, // Note: Shallow copy of BeamProperties
                MaxWheelLoad = this.MaxWheelLoad,
                RunwayBeamWeight = this.RunwayBeamWeight,
                LateralLoad = this.LateralLoad,
                LongitudinalLoad = this.LongitudinalLoad,
                ColumnMoment = this.ColumnMoment,
                FoundationMoment = this.FoundationMoment,
                LateralOTM = this.LateralOTM,
                LongitudinalOTM = this.LongitudinalOTM,
                MaxVerticalLoad = this.MaxVerticalLoad,
                ColumnLoadFoundation = this.ColumnLoadFoundation,
                LateralDeflectionPass = this.LateralDeflectionPass,
                LongitudinalDeflectionPass = this.LongitudinalDeflectionPass,
                StressCheckPass = this.StressCheckPass,
                AxialCheckPass = this.AxialCheckPass,
                OverallPass = this.OverallPass,
                WheelbaseSpanRatio = this.WheelbaseSpanRatio,
                ImpactFactor = this.ImpactFactor,
                GirderWeight = this.GirderWeight,
                PanelWeight = this.PanelWeight,
                EndTruckWeight = this.EndTruckWeight,
                TotalBeamWeight = this.TotalBeamWeight,
                AnalysisDate = this.AnalysisDate
            };
        }
    }
}
        #endregion