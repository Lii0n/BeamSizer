// BeamSizerConfig.cs - Enhanced version with all user-input-dependent calculations
// Moved duplicate calculations from BeamCalculator to eliminate redundancy

using System;

namespace BeamSizing
{
    /// <summary>
    /// Immutable configuration struct for Beam sizing parameters.
    /// Now includes all calculations that depend only on user input to eliminate duplication.
    /// </summary>
    public readonly struct BeamSizerConfig
    {
        // Input parameters - all readonly for immutability
        public double RatedCapacity { get; }        // P (lbs)
        public double WeightHoistTrolley { get; }   // H (lbs)

        // Beam weight components
        public double GirderWeight { get; }         // (lbs)
        public double PanelWeight { get; }          // (lbs)
        public double EndTruckWeight { get; }       // (lbs)
        public double WeightBeam { get; }          // C (lbs) - Sum of girder, panel, and end truck weights

        public int NumCols { get; }                 // Per side 
        public double RailHeight { get; }           // (ft)
        public double WheelBase { get; }            // (ft)
        public double SupportCenters { get; }       // Distance between runway beam supports (ft)
        public bool Freestanding { get; }           // Column support condition
        public bool Capped { get; }                 // Use capped beam system
        public double BridgeSpan { get; }           // Distance between runway beams (Beam span) (ft)
        public double HoistSpeed { get; }           // (ft/min)

        // Computed properties - calculated once during construction
        public double ImpactFactor { get; }
        public double MaxWheelLoad { get; }
        public double WheelbaseSpanRatio { get; }

        // NEW: Pre-calculated loads that depend only on user input
        public double LateralLoad { get; }          // 20% of (crane capacity + hoist/trolley weight)
        public double LongitudinalLoad { get; }     // 10% of max wheel load
        public double RailHeightInches { get; }     // Rail height converted to inches
        public double EffectiveLengthFactor { get; } // Factor for freestanding vs braced columns
        public double EffectiveLength { get; }      // Rail height * effective length factor

        /// <summary>
        /// Creates a new immutable Beam sizing configuration.
        /// All validation and calculations occur during construction.
        /// </summary>
        public BeamSizerConfig(
            double ratedCapacity,
            double weightHoistTrolley,
            double girderWeight,
            double panelWeight,
            double endTruckWeight,
            int numCols,
            double railHeight,
            double wheelBase,
            double supportCenters,
            bool freestanding,
            bool capped,
            double bridgeSpan,
            double hoistSpeed = 0)
        {
            // Calculate total Beam weight from components
            double weightBeam = girderWeight + panelWeight + endTruckWeight;

            // Validate all inputs before assignment
            ValidateInputs(ratedCapacity, weightHoistTrolley, weightBeam, numCols,
                          railHeight, wheelBase, supportCenters, bridgeSpan, hoistSpeed);

            // Assign input parameters
            RatedCapacity = ratedCapacity;
            WeightHoistTrolley = weightHoistTrolley;
            GirderWeight = girderWeight;
            PanelWeight = panelWeight;
            EndTruckWeight = endTruckWeight;
            WeightBeam = weightBeam;
            NumCols = numCols;
            RailHeight = railHeight;
            WheelBase = wheelBase;
            SupportCenters = supportCenters;
            Freestanding = freestanding;
            Capped = capped;
            BridgeSpan = bridgeSpan;
            HoistSpeed = hoistSpeed;

            // Calculate derived values once
            ImpactFactor = CalculateImpactFactor(hoistSpeed);
            WheelbaseSpanRatio = wheelBase / supportCenters;
            MaxWheelLoad = ((ImpactFactor * ratedCapacity) / 2.0) +
                          (weightHoistTrolley / 2.0) +
                          (weightBeam / 4.0);

            // NEW: Calculate loads that depend only on user input
            LateralLoad = 0.2 * (ratedCapacity + weightHoistTrolley);
            LongitudinalLoad = 0.1 * MaxWheelLoad;
            RailHeightInches = railHeight * 12.0;
            EffectiveLengthFactor = freestanding ? 2.0 : 0.5;
            EffectiveLength = railHeight * EffectiveLengthFactor;

            Console.WriteLine($"DEBUG: Created config with BridgeSpan={BridgeSpan:F1} ft, SupportCenters={SupportCenters:F1} ft");
            Console.WriteLine($"DEBUG: Pre-calculated LateralLoad={LateralLoad:F0} lbs, LongitudinalLoad={LongitudinalLoad:F0} lbs");
        }

        /// <summary>
        /// Calculate column moment from lateral load (depends only on config values)
        /// </summary>
        public double CalculateColumnMoment() => RailHeightInches * LateralLoad;

        /// <summary>
        /// Calculate foundation moment from longitudinal load (depends only on config values)
        /// </summary>
        public double CalculateFoundationMoment() => RailHeightInches * LongitudinalLoad;

        /// <summary>
        /// Calculate runway beam weight for a given beam (depends on config + beam)
        /// </summary>
        public double CalculateRunwayBeamWeight(BeamProperties beam)
        {
            if (beam == null)
                throw new ArgumentNullException(nameof(beam), "Beam cannot be null");
            return beam.Weight * BridgeSpan;
        }

        /// <summary>
        /// Calculate maximum vertical load on runway system (depends on config + runway beam weight)
        /// </summary>
        public double CalculateMaxVerticalLoad(double runwayBeamWeight) =>
            RatedCapacity + WeightBeam + WeightHoistTrolley + runwayBeamWeight;

        /// <summary>
        /// Calculate column load on foundation (static calculation)
        /// </summary>
        public static double CalculateColumnLoadFoundation(double maxVerticalLoad) =>
            (maxVerticalLoad + 2500) / 1000.0; // Convert to kips with safety factor

        /// <summary>
        /// Convert moment to overturning moment in kip-ft (static calculation)
        /// </summary>
        public static double ConvertToOTM(double momentLbIn) =>
            momentLbIn / (1000.0 * 12.0);

        /// <summary>
        /// Validates all input parameters and throws descriptive exceptions for invalid values.
        /// </summary>
        private static void ValidateInputs(double ratedCapacity, double weightHoistTrolley, double weightBeam,
                                          int numCols, double railHeight, double wheelBase,
                                          double supportCenters, double bridgeSpan, double hoistSpeed)
        {
            if (ratedCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(ratedCapacity),
                    "Rated capacity must be positive");

            if (ratedCapacity > 80000)
                throw new ArgumentOutOfRangeException(nameof(ratedCapacity),
                    "Rated capacity exceeds typical Beam limits (80,000 lbs)");

            if (weightHoistTrolley <= 0)
                throw new ArgumentOutOfRangeException(nameof(weightHoistTrolley),
                    "Hoist/trolley weight must be positive");

            if (weightBeam <= 0)
                throw new ArgumentOutOfRangeException(nameof(weightBeam),
                    "Beam weight must be positive");

            if (numCols <= 1)
                throw new ArgumentOutOfRangeException(nameof(numCols),
                    "Number of columns must be at least 2");

            if (railHeight <= 0 || railHeight > 100)
                throw new ArgumentOutOfRangeException(nameof(railHeight),
                    "Rail height must be between 0 and 100 feet");

            if (wheelBase <= 0 || wheelBase > 50)
                throw new ArgumentOutOfRangeException(nameof(wheelBase),
                    "Wheelbase must be between 0 and 50 feet");

            if (supportCenters <= 0 || supportCenters > 150)
                throw new ArgumentOutOfRangeException(nameof(supportCenters),
                    "Support centers must be between 0 and 150 feet");

            if (bridgeSpan <= 0 || bridgeSpan > 120)
                throw new ArgumentOutOfRangeException(nameof(bridgeSpan),
                    "Span length must be between 0 and 120 feet");

            if (hoistSpeed < 0 || hoistSpeed > 500)
                throw new ArgumentOutOfRangeException(nameof(hoistSpeed),
                    "Hoist speed must be between 0 and 500 ft/min");

            // Additional validation for ratios and combinations
            if (wheelBase > supportCenters)
                throw new ArgumentException(
                    "Wheelbase cannot be greater than support centers");

            if (railHeight < 8)
                throw new ArgumentOutOfRangeException(nameof(railHeight),
                    "Rail height should be at least 8 feet for practical Beam operation");
        }

        /// <summary>
        /// Calculates impact factor based on hoist speed.
        /// </summary>
        private static double CalculateImpactFactor(double hoistSpeed)
        {
            // Default impact factor when no hoist speed is specified
            if (hoistSpeed <= 0)
                return 1.15;
            else
                return (0.005 * hoistSpeed) + 1;
        }

        /// <summary>
        /// Gets a formatted analysis summary for debugging and validation.
        /// </summary>
        public string GetAnalysisSummary()
        {
            return $"""
                Beam Configuration Summary:
                  Capacity: {RatedCapacity:N0} lbs
                  Wheelbase: {WheelBase} ft, Support Centers: {SupportCenters} ft
                  Wheelbase/Support Ratio: {WheelbaseSpanRatio:F4}
                  Impact Factor: {ImpactFactor:F3}
                  Max Wheel Load: {MaxWheelLoad:F0} lbs
                  Lateral Load: {LateralLoad:F0} lbs
                  Longitudinal Load: {LongitudinalLoad:F0} lbs
                  Rail Height: {RailHeight} ft ({RailHeightInches} in)
                  Effective Length: {EffectiveLength:F1} ft (Factor: {EffectiveLengthFactor})
                  Beam System: {(Capped ? "Capped" : "Uncapped")}
                  Column Type: {(Freestanding ? "Freestanding" : "Braced")}
                  Hoist Speed: {(HoistSpeed > 0 ? $"{HoistSpeed} ft/min" : "Default")}
                  Beam Weight Components:
                    Girder: {GirderWeight:N0} lbs
                    Panel: {PanelWeight:N0} lbs
                    End Truck: {EndTruckWeight:N0} lbs
                    Total: {WeightBeam:N0} lbs
                """;
        }

        /// <summary>
        /// Provides a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"BeamSizerConfig[Capacity={RatedCapacity:N0}, " +
                   $"Span={SupportCenters}ft, " +
                   $"System={(Capped ? "Capped" : "Uncapped")}]";
        }
    }
}