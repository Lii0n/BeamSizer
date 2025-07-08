// BeamSizerConfig.cs - Cleaned up version with only used functionality
// Removed unused methods while keeping core immutable configuration

using System;

namespace BeamSizing
{
    /// <summary>
    /// Immutable configuration struct for Beam sizing parameters.
    /// Optimized for server environments with multiple concurrent users.
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

        /// <summary>
        /// Creates a new immutable Beam sizing configuration.
        /// All validation occurs during construction.
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

            Console.WriteLine($"DEBUG: Created config with BridgeSpan={BridgeSpan:F1} ft, SupportCenters={SupportCenters:F1} ft");
        }

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
                  Bridge Span (distance between runway beams): {BridgeSpan:F1} ft
                  Wheelbase/Support Ratio: {WheelbaseSpanRatio:F4}
                  Impact Factor: {ImpactFactor:F3}
                  Max Wheel Load: {MaxWheelLoad:F0} lbs
                  Beam System: {(Capped ? "Capped" : "Uncapped")}
                  Column Type: {(Freestanding ? "Freestanding" : "Braced")}
                  Rail Height: {RailHeight} ft
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
                   $"Span={BridgeSpan}ft, " +
                   $"System={(Capped ? "Capped" : "Uncapped")}]";
        }
    }
}