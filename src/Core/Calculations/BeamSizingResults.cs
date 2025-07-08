// BeamSizingResults.cs - Cleaned up version with only used properties
// Removed all unused methods while preserving data container functionality

using System;
using System.Collections.Generic;

namespace BeamSizing
{
    /// <summary>
    /// Contains all calculated values and check results from beam sizing analysis
    /// This class serves as a data container - all methods were unused and removed
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
        /// Maximum wheel load from crane (lbs)
        /// </summary>
        public double MaxWheelLoad { get; set; }

        /// <summary>
        /// Total weight of runway beam (lbs)
        /// </summary>
        public double RunwayBeamWeight { get; set; }

        /// <summary>
        /// Lateral load on columns (lbs)
        /// 20% of (crane capacity + hoist/trolley weight)
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
        /// Impact factor applied to crane loads
        /// </summary>
        public double ImpactFactor { get; set; }

        /// <summary>
        /// Analysis timestamp
        /// </summary>
        public DateTime AnalysisDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Crane component weights from config
        /// </summary>
        public double GirderWeight { get; set; }
        public double PanelWeight { get; set; }
        public double EndTruckWeight { get; set; }
        public double TotalBeamWeight { get; set; }
        #endregion
    }
}