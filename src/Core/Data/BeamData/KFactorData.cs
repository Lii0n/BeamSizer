// KFactorData.cs - Cleaned up version with only used functions
// Removed unused functions while preserving core K-factor lookup functionality

using System;
using System.Collections.Generic;

namespace BeamSizing.Data
{
    public static class KFactorData
    {
        /// <summary>
        /// Engineering table for k1 and k2 factors based on A/L ratio (wheelbase/support centers)
        /// Format: (a_over_l_ratio, k1, k2)
        /// This matches the correct engineering_tables.py values
        /// </summary>
        private static readonly List<(double ratio, double k1, double k2)> KFactorTable =
            new List<(double, double, double)>
        {
            // A/L Ratio,  k1,     k2
            (0.00, 2.000, 2.000),
            (0.05, 1.902, 1.950),
            (0.10, 1.805, 1.900),
            (0.15, 1.712, 1.850),
            (0.20, 1.620, 1.800),
            (0.25, 1.532, 1.750),
            (0.30, 1.445, 1.700),
            (0.35, 1.362, 1.650),
            (0.40, 1.280, 1.600),
            (0.45, 1.202, 1.550),
            (0.50, 1.125, 1.500),
            (0.55, 1.052, 1.450),
            (0.60, 1.000, 1.400),
            (0.65, 1.000, 1.350),
            (0.70, 1.000, 1.300),
            (0.75, 1.000, 1.250),
            (0.80, 1.000, 1.200),
            (0.85, 1.000, 1.150),
            (0.90, 1.000, 1.100),
            (0.95, 1.000, 1.050),
            (1.00, 1.000, 1.000)
        };

        /// <summary>
        /// Get K-factors for a given wheelbase to support centers ratio (A/L ratio)
        /// Uses linear interpolation between table values for accuracy
        /// USED BY: DataLoader.GetKFactors -> BeamCalculator.FindKFactors
        /// </summary>
        /// <param name="aOverLRatio">The ratio of wheelbase (A) to support centers (L)</param>
        /// <returns>Tuple containing (k1, k2) factors</returns>
        public static (double k1, double k2) GetKFactors(double aOverLRatio)
        {
            // Handle edge cases - clamp to table bounds
            if (aOverLRatio <= KFactorTable[0].ratio)
            {
                return (KFactorTable[0].k1, KFactorTable[0].k2);
            }

            if (aOverLRatio >= KFactorTable[KFactorTable.Count - 1].ratio)
            {
                var lastEntry = KFactorTable[KFactorTable.Count - 1];
                return (lastEntry.k1, lastEntry.k2);
            }

            // Find the two points to interpolate between
            for (int i = 0; i < KFactorTable.Count - 1; i++)
            {
                var (ratio1, k1_1, k2_1) = KFactorTable[i];
                var (ratio2, k1_2, k2_2) = KFactorTable[i + 1];

                if (ratio1 <= aOverLRatio && aOverLRatio <= ratio2)
                {
                    // Linear interpolation
                    // k = k1 + (k2 - k1) * (x - x1) / (x2 - x1)
                    double interpolationFactor = (aOverLRatio - ratio1) / (ratio2 - ratio1);

                    double k1 = k1_1 + (k1_2 - k1_1) * interpolationFactor;
                    double k2 = k2_1 + (k2_2 - k2_1) * interpolationFactor;

                    return (Math.Round(k1, 3), Math.Round(k2, 3));
                }
            }

            // Fallback (should not reach here with proper table)
            return (1.5, 1.5);
        }
    }
}