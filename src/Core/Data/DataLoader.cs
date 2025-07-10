// DataLoader.cs - COMPLETE REPLACEMENT for your existing DataLoader.cs file
// This maintains all your existing method names while adding optimizations

using System;
using System.Collections.Generic;
using System.Linq;
using BeamSizing.Data;

namespace BeamSizing
{
    public static class DataLoader
    {
        // OPTIMIZED: Pre-built lookup tables - created once at startup
        private static readonly Lazy<Dictionary<string, BeamProperties>> _uncappedBeamLookup =
            new Lazy<Dictionary<string, BeamProperties>>(() =>
                UncappedBeamData.UncappedBeams.ToDictionary(b => b.Designation, b => b));

        private static readonly Lazy<Dictionary<string, BeamProperties>> _cappedBeamLookup =
            new Lazy<Dictionary<string, BeamProperties>>(() =>
                CappedBeamData.CappedBeams.Cast<BeamProperties>().ToDictionary(b => b.Designation, b => b));

        // Capacity lookups - flattened for instant access
        private static readonly Lazy<Dictionary<(string beam, int span), int>> _uncappedCapacityLookup =
            new Lazy<Dictionary<(string, int), int>>(() => BuildFlatCapacityLookup(false));

        private static readonly Lazy<Dictionary<(string beam, int span), int>> _cappedCapacityLookup =
            new Lazy<Dictionary<(string, int), int>>(() => BuildFlatCapacityLookup(true));

        // Sorted beam lists for range queries - sorted once, reused forever
        private static readonly Lazy<List<BeamProperties>> _sortedUncappedBeams =
            new Lazy<List<BeamProperties>>(() =>
                UncappedBeamData.UncappedBeams.OrderBy(b => b.Weight).ToList());

        private static readonly Lazy<List<BeamProperties>> _sortedCappedBeams =
            new Lazy<List<BeamProperties>>(() =>
                CappedBeamData.CappedBeams.Cast<BeamProperties>().OrderBy(b => b.Weight).ToList());

        // Span availability index - know instantly what spans are available for each beam
        private static readonly Lazy<Dictionary<string, HashSet<int>>> _beamSpanIndex =
            new Lazy<Dictionary<string, HashSet<int>>>(() => BuildBeamSpanIndex());

        // Interpolation cache for repeated calculations
        private static readonly Dictionary<(string, double, bool), double> _interpolationCache
            = new Dictionary<(string, double, bool), double>();

        #region Public Properties (Maintains compatibility with existing code)

        public static Dictionary<string, BeamProperties> UncappedBeams => _uncappedBeamLookup.Value;
        public static Dictionary<string, BeamProperties> CappedBeams => _cappedBeamLookup.Value;
        public static Dictionary<(string beam, int span), int> UncappedCapacities => _uncappedCapacityLookup.Value;
        public static Dictionary<(string beam, int span), int> CappedCapacities => _cappedCapacityLookup.Value;

        #endregion

        #region Main Functions (Drop-in replacements for your existing code)

        /// <summary>
        /// Get K-factors based on wheelbase to span ratio - UNCHANGED from your original
        /// USED BY: BeamCalculator.FindKFactors -> BeamCalculator.PerformFullAnalysis
        /// </summary>
        public static (double k1, double k2) GetKFactors(double wheelbaseToSpanRatio)
        {
            return KFactorData.GetKFactors(wheelbaseToSpanRatio);
        }

        /// <summary>
        /// OPTIMIZED: Find adequate beams - Drop-in replacement for your existing method
        /// USED BY: BeamCalculator.FindBeamSizeWithCandidates -> BeamCalculator.PerformFullAnalysis
        /// </summary>
        public static List<BeamProperties> FindTopAdequateBeams(
            double requiredCapacity,
            double spanLength,
            bool capped = false,
            int topN = 5)
        {
            Console.WriteLine($"DEBUG: FindTopAdequateBeams called with ECL = {requiredCapacity:F0} lbs, span = {spanLength:F1} ft, capped = {capped}");

            var startTime = DateTime.Now;

            // Validate inputs
            if (requiredCapacity <= 0)
            {
                Console.WriteLine($"ERROR: Invalid required capacity {requiredCapacity}");
                return new List<BeamProperties>();
            }

            if (spanLength <= 0)
            {
                Console.WriteLine($"ERROR: Invalid span length {spanLength}. Cannot proceed with beam selection.");
                return new List<BeamProperties>();
            }

            // Use pre-sorted lists instead of sorting every time
            var sortedBeams = capped ? _sortedCappedBeams.Value : _sortedUncappedBeams.Value;
            var spanIndex = _beamSpanIndex.Value;

            var adequateBeams = new List<(BeamProperties beam, double capacity, double utilization)>();

            // OPTIMIZATION 1: Pre-filter beams that have data for this span range
            var targetSpan = (int)Math.Round(spanLength);
            var spanRange = GetSpanRange(spanLength);

            foreach (var beam in sortedBeams)
            {
                // OPTIMIZATION 2: Skip beams that don't have capacity data for this span range
                if (!spanIndex.TryGetValue(beam.Designation, out var availableSpans))
                    continue;

                if (!availableSpans.Any(s => s >= spanRange.min && s <= spanRange.max))
                    continue;

                // OPTIMIZATION 3: Use optimized interpolation method
                double beamCapacity = GetInterpolatedLoadCapacity(beam.Designation, spanLength, capped);

                if (beamCapacity >= requiredCapacity)
                {
                    double utilization = (requiredCapacity / beamCapacity) * 100.0;
                    adequateBeams.Add((beam, beamCapacity, utilization));

                    Console.WriteLine($"DEBUG: {beam.Designation} - Capacity: {beamCapacity:F0} lbs, Utilization: {utilization:F1}%");

                    // OPTIMIZATION 4: Early exit when we have enough candidates
                    // Since beams are pre-sorted by weight, we can stop after finding enough
                    if (adequateBeams.Count >= topN * 2) // Get a few extra for safety
                        break;
                }
            }

            var result = adequateBeams
                .OrderBy(x => x.beam.Weight) // Final sort by weight
                .Take(topN)
                .Select(x => x.beam)
                .ToList();

            var elapsed = DateTime.Now - startTime;
            Console.WriteLine($"SUCCESS: Found {result.Count} adequate beams for ECL = {requiredCapacity:F0} lbs at span = {spanLength:F1} ft in {elapsed.TotalMilliseconds:F1}ms");

            return result;
        }

        /// <summary>
        /// OPTIMIZED: Interpolated capacity lookup with caching - Drop-in replacement
        /// USED BY: DataLoader.FindTopAdequateBeams and various capacity calculations
        /// </summary>
        public static double GetInterpolatedLoadCapacity(string beamDesignation, double spanLength, bool capped = false)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(beamDesignation))
            {
                Console.WriteLine("ERROR: Invalid beam designation for interpolation");
                return 0;
            }

            if (spanLength <= 0)
            {
                Console.WriteLine($"ERROR: Invalid span length {spanLength} for beam {beamDesignation}");
                return 0;
            }

            // OPTIMIZATION 1: Check cache first
            var cacheKey = (beamDesignation, Math.Round(spanLength, 1), capped);
            if (_interpolationCache.TryGetValue(cacheKey, out var cachedResult))
                return cachedResult;

            // OPTIMIZATION 2: Fast exact match check
            int exactSpan = (int)Math.Round(spanLength);
            if (Math.Abs(spanLength - exactSpan) < 0.001)
            {
                var exactCapacity = GetBeamCapacity(beamDesignation, exactSpan, capped);
                if (exactCapacity > 0)
                {
                    _interpolationCache[cacheKey] = exactCapacity;
                    Console.WriteLine($"DEBUG: Found exact capacity {exactCapacity} lbs for {beamDesignation} at {exactSpan} ft");
                    return exactCapacity;
                }
            }

            // OPTIMIZATION 3: Use span index for faster range finding
            var spanIndex = _beamSpanIndex.Value;
            if (!spanIndex.TryGetValue(beamDesignation, out var availableSpans))
            {
                Console.WriteLine($"ERROR: Beam {beamDesignation} not found in {(capped ? "capped" : "uncapped")} capacity data");
                return 0;
            }

            var sortedSpans = availableSpans.OrderBy(s => s).ToList();

            // Check if span is within available range
            if (spanLength < sortedSpans.Min())
            {
                Console.WriteLine($"WARNING: Span {spanLength} ft is below minimum available span {sortedSpans.Min()} ft for {beamDesignation}");
                return 0;
            }

            if (spanLength > sortedSpans.Max())
            {
                Console.WriteLine($"WARNING: Span {spanLength} ft is above maximum available span {sortedSpans.Max()} ft for {beamDesignation}");
                return 0;
            }

            // Find interpolation bounds
            var lowerSpan = sortedSpans.Where(s => s <= spanLength).DefaultIfEmpty(0).Max();
            var upperSpan = sortedSpans.Where(s => s >= spanLength).DefaultIfEmpty(0).Min();

            if (lowerSpan == 0 || upperSpan == 0 || lowerSpan == upperSpan)
            {
                var result = lowerSpan > 0 ? GetBeamCapacity(beamDesignation, lowerSpan, capped) : 0;
                _interpolationCache[cacheKey] = result;
                Console.WriteLine($"DEBUG: Table match at {lowerSpan} ft = {result:N0} lbs for {beamDesignation}");
                return result;
            }

            // Linear interpolation
            var lowerCapacity = GetBeamCapacity(beamDesignation, lowerSpan, capped);
            var upperCapacity = GetBeamCapacity(beamDesignation, upperSpan, capped);

            double interpolationFactor = (spanLength - lowerSpan) / (upperSpan - lowerSpan);
            double interpolatedCapacity = lowerCapacity + interpolationFactor * (upperCapacity - lowerCapacity);

            _interpolationCache[cacheKey] = interpolatedCapacity;

            Console.WriteLine($"DEBUG: Interpolated {beamDesignation} at {spanLength:F1} ft: " +
                            $"Between {lowerSpan}ft({lowerCapacity} lbs) and {upperSpan}ft({upperCapacity} lbs) = {interpolatedCapacity:F0} lbs");

            return interpolatedCapacity;
        }

        /// <summary>
        /// OPTIMIZED: Get beam capacity - O(1) instead of nested loops
        /// USED BY: GetInterpolatedLoadCapacity for exact span lookups
        /// </summary>
        public static int GetBeamCapacity(string designation, int span, bool capped = false)
        {
            var lookup = capped ? CappedCapacities : UncappedCapacities;
            return lookup.TryGetValue((designation, span), out var capacity) ? capacity : 0;
        }

        /// <summary>
        /// Get table validation information for engineering review - UNCHANGED
        /// USED BY: Engineering validation and debugging
        /// </summary>
        public static string GetTableValidationInfo(string beamDesignation, double span, bool capped)
        {
            var lookup = capped ? CappedCapacities : UncappedCapacities;

            // Check if we have exact span match
            int exactSpan = (int)Math.Round(span);
            bool hasExactMatch = lookup.ContainsKey((beamDesignation, exactSpan));

            if (hasExactMatch && Math.Abs(span - exactSpan) < 0.001)
            {
                // Exact match case
                int exactCapacity = lookup[(beamDesignation, exactSpan)];
                return $"Exact table value: {exactSpan} ft → {exactCapacity:N0} lbs";
            }
            else
            {
                // Interpolation case - show the two points being interpolated
                var spanIndex = _beamSpanIndex.Value;
                if (!spanIndex.TryGetValue(beamDesignation, out var availableSpans))
                {
                    return "❌ ERROR: No table data found";
                }

                var beamSpans = availableSpans.OrderBy(s => s).ToList();

                if (beamSpans.Count == 0)
                {
                    return "❌ ERROR: No table data found";
                }

                var lowerSpan = beamSpans.Where(s => s <= span).DefaultIfEmpty(0).Max();
                var upperSpan = beamSpans.Where(s => s >= span).DefaultIfEmpty(0).Min();

                if (lowerSpan == 0 || upperSpan == 0)
                {
                    return "❌ ERROR: Span outside table range";
                }

                if (lowerSpan == upperSpan)
                {
                    // Exact match after all
                    int capacity = lookup[(beamDesignation, lowerSpan)];
                    return $"Exact table value: {lowerSpan} ft → {capacity:N0} lbs";
                }
                else
                {
                    // Show interpolation points with more detail
                    int lowerCapacity = lookup[(beamDesignation, lowerSpan)];
                    int upperCapacity = lookup[(beamDesignation, upperSpan)];
                    double interpolationFactor = (span - lowerSpan) / (upperSpan - lowerSpan);

                    return $"Interpolated between: {lowerSpan}ft({lowerCapacity:N0} lbs) ↔ {upperSpan}ft({upperCapacity:N0} lbs), factor={interpolationFactor:F3}";
                }
            }
        }

        #endregion

        #region Helper Methods

        private static Dictionary<(string, int), int> BuildFlatCapacityLookup(bool capped)
        {
            var lookup = new Dictionary<(string, int), int>();
            var sourceData = capped ? CappedCapacityData.LoadCapacities : UncappedCapacityData.LoadCapacities;

            foreach (var beamCapacities in sourceData)
            {
                foreach (var spanCapacity in beamCapacities.Value)
                {
                    lookup[(beamCapacities.Key, spanCapacity.Key)] = spanCapacity.Value;
                }
            }

            Console.WriteLine($"Built {(capped ? "capped" : "uncapped")} capacity lookup with {lookup.Count} entries");
            return lookup;
        }

        private static Dictionary<string, HashSet<int>> BuildBeamSpanIndex()
        {
            var index = new Dictionary<string, HashSet<int>>();

            // Build index for uncapped beams
            foreach (var entry in UncappedCapacityData.LoadCapacities)
            {
                index[entry.Key] = new HashSet<int>(entry.Value.Keys);
            }

            // Build index for capped beams
            foreach (var entry in CappedCapacityData.LoadCapacities)
            {
                index[entry.Key] = new HashSet<int>(entry.Value.Keys);
            }

            Console.WriteLine($"Built beam span index with {index.Count} beam entries");
            return index;
        }

        private static (int min, int max) GetSpanRange(double spanLength)
        {
            int baseSpan = (int)Math.Floor(spanLength);
            return (Math.Max(baseSpan - 2, 10), baseSpan + 4);
        }

        #endregion

        #region Performance Monitoring


        public static void PrintPerformanceStats()
        {
            Console.WriteLine("=== BEAM SEARCH PERFORMANCE STATS ===");
            Console.WriteLine($"Uncapped beams loaded: {UncappedBeams.Count}");
            Console.WriteLine($"Capped beams loaded: {CappedBeams.Count}");
            Console.WriteLine($"Uncapped capacities: {UncappedCapacities.Count}");
            Console.WriteLine($"Capped capacities: {CappedCapacities.Count}");
            Console.WriteLine($"Interpolation cache size: {_interpolationCache.Count}");
            Console.WriteLine($"Beam span index size: {_beamSpanIndex.Value.Count}");
        }

        #endregion
    }
}