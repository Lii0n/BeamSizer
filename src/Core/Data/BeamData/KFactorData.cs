// KFactorData.cs
// K-factor lookup table for load distribution calculations
// Translated from engineering_tables.py with correct values

using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Translated from Python engineering_tables.py
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

        /// <summary>
        /// Get the closest tabulated ratio for a given input ratio
        /// Useful for debugging and validation
        /// </summary>
        /// <param name="ratio">Input A/L ratio</param>
        /// <returns>Closest tabulated ratio and its factors</returns>
        public static (double ratio, double k1, double k2) GetClosestTabulatedValues(double ratio)
        {
            var closestEntry = KFactorTable.OrderBy(entry => Math.Abs(entry.ratio - ratio)).First();
            return closestEntry;
        }

        /// <summary>
        /// Get all available ratios and their corresponding factors
        /// Useful for debugging and displaying the lookup table
        /// </summary>
        /// <returns>List of all ratio/factor combinations</returns>
        public static List<(double ratio, double k1, double k2)> GetAllKFactors()
        {
            return new List<(double, double, double)>(KFactorTable);
        }

        /// <summary>
        /// Validates that a ratio is within the supported range
        /// </summary>
        /// <param name="ratio">Wheelbase to support centers ratio</param>
        /// <returns>True if ratio is within supported range</returns>
        public static bool IsRatioSupported(double ratio)
        {
            return ratio >= KFactorTable[0].ratio && ratio <= KFactorTable[KFactorTable.Count - 1].ratio;
        }

        /// <summary>
        /// Gets engineering guidance for ratios
        /// </summary>
        /// <param name="ratio">Wheelbase to support centers ratio</param>
        /// <returns>Engineering guidance string</returns>
        public static string GetEngineeringGuidance(double ratio)
        {
            if (ratio < KFactorTable[0].ratio)
                return $"Ratio {ratio:F3} is below minimum supported value ({KFactorTable[0].ratio:F2}). " +
                       "Consider increasing wheelbase or reducing support centers. K-factors clamped to minimum values.";

            if (ratio > KFactorTable[KFactorTable.Count - 1].ratio)
                return $"Ratio {ratio:F3} is above maximum supported value ({KFactorTable[KFactorTable.Count - 1].ratio:F2}). " +
                       "This may indicate a very short support span or very long wheelbase. Verify Beam geometry. K-factors clamped to maximum values.";

            return $"Ratio {ratio:F3} is within normal range for overhead Beams.";
        }

        /// <summary>
        /// Update the k-factor table with new engineering data
        /// </summary>
        /// <param name="newTable">List of (a_over_l_ratio, k1, k2) entries</param>
        public static void UpdateKFactorTable(List<(double ratio, double k1, double k2)> newTable)
        {
            if (newTable == null || newTable.Count == 0)
                throw new ArgumentException("New table cannot be null or empty", nameof(newTable));

            // Sort by A/L ratio to ensure proper interpolation
            var sortedTable = newTable.OrderBy(entry => entry.ratio).ToList();

            // Validate that ratios are unique and increasing
            for (int i = 1; i < sortedTable.Count; i++)
            {
                if (sortedTable[i].ratio <= sortedTable[i - 1].ratio)
                {
                    throw new ArgumentException($"Duplicate or decreasing ratio found at index {i}: {sortedTable[i].ratio}", nameof(newTable));
                }
            }

            // Replace the table (note: this would require making KFactorTable non-readonly)
            // For now, this method serves as a template for future extensibility
            throw new NotImplementedException("K-factor table updates require code modification for thread safety");
        }

        /// <summary>
        /// Validates the internal K-factor table for consistency
        /// </summary>
        /// <returns>True if table is valid</returns>
        public static bool ValidateKFactorTable()
        {
            try
            {
                // Check table is not empty
                if (KFactorTable.Count == 0)
                {
                    Console.WriteLine("K-factor table validation failed: Table is empty");
                    return false;
                }

                // Check ratios are in ascending order
                for (int i = 1; i < KFactorTable.Count; i++)
                {
                    if (KFactorTable[i].ratio <= KFactorTable[i - 1].ratio)
                    {
                        Console.WriteLine($"K-factor table validation failed: Non-ascending ratio at index {i}");
                        return false;
                    }
                }

                // Check K-factors are positive
                foreach (var entry in KFactorTable)
                {
                    if (entry.k1 <= 0 || entry.k2 <= 0)
                    {
                        Console.WriteLine($"K-factor table validation failed: Non-positive K-factor at ratio {entry.ratio}");
                        return false;
                    }
                }

                Console.WriteLine($"K-factor table validation passed: {KFactorTable.Count} entries loaded successfully");
                Console.WriteLine($"  Ratio range: {KFactorTable[0].ratio:F2} to {KFactorTable[KFactorTable.Count - 1].ratio:F2}");
                Console.WriteLine($"  K1 range: {KFactorTable.Min(e => e.k1):F3} to {KFactorTable.Max(e => e.k1):F3}");
                Console.WriteLine($"  K2 range: {KFactorTable.Min(e => e.k2):F3} to {KFactorTable.Max(e => e.k2):F3}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"K-factor table validation failed with exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test the interpolation logic with known values
        /// </summary>
        /// <returns>True if interpolation tests pass</returns>
        public static bool TestInterpolation()
        {
            try
            {
                // Test exact table values
                var (k1_exact, k2_exact) = GetKFactors(0.25);
                if (Math.Abs(k1_exact - 1.532) > 0.001 || Math.Abs(k2_exact - 1.750) > 0.001)
                {
                    Console.WriteLine($"Interpolation test failed: Expected (1.532, 1.750), got ({k1_exact}, {k2_exact})");
                    return false;
                }

                // Test interpolated value between 0.25 (1.532, 1.750) and 0.30 (1.445, 1.700)
                var (k1_interp, k2_interp) = GetKFactors(0.275); // Midpoint
                double expected_k1 = 1.532 + (1.445 - 1.532) * 0.5; // Should be 1.4885
                double expected_k2 = 1.750 + (1.700 - 1.750) * 0.5; // Should be 1.725

                if (Math.Abs(k1_interp - expected_k1) > 0.001 || Math.Abs(k2_interp - expected_k2) > 0.001)
                {
                    Console.WriteLine($"Interpolation test failed: Expected ({expected_k1:F3}, {expected_k2:F3}), got ({k1_interp}, {k2_interp})");
                    return false;
                }

                Console.WriteLine("K-factor interpolation tests passed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Interpolation test failed with exception: {ex.Message}");
                return false;
            }
        }
    }
}