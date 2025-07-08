// DataLoader.cs
// Central class for loading all Beam sizing data
// FIXED: ECL comparison now sorts by weight only when span is specified

using System;
using System.Collections.Generic;
using System.Linq;
using BeamSizing.Data;

namespace BeamSizing
{
    public static class DataLoader
    {
        // Lazy loading for performance - data loaded only when first accessed
        private static readonly Lazy<Dictionary<string, BeamProperties>> _uncappedBeamLookup =
            new Lazy<Dictionary<string, BeamProperties>>(() =>
                UncappedBeamData.UncappedBeams.ToDictionary(b => b.Designation, b => b));

        private static readonly Lazy<Dictionary<string, BeamProperties>> _cappedBeamLookup =
            new Lazy<Dictionary<string, BeamProperties>>(() =>
                CappedBeamData.CappedBeams.Cast<BeamProperties>().ToDictionary(b => b.Designation, b => b));

        private static readonly Lazy<Dictionary<(string beam, int span), int>> _uncappedCapacityLookup =
            new Lazy<Dictionary<(string, int), int>>(() => BuildUncappedCapacityLookup());

        private static readonly Lazy<Dictionary<(string beam, int span), int>> _cappedCapacityLookup =
            new Lazy<Dictionary<(string, int), int>>(() => BuildCappedCapacityLookup());

        // Public properties for accessing data
        public static Dictionary<string, BeamProperties> UncappedBeams => _uncappedBeamLookup.Value;
        public static Dictionary<string, BeamProperties> CappedBeams => _cappedBeamLookup.Value;
        public static Dictionary<(string beam, int span), int> UncappedCapacities => _uncappedCapacityLookup.Value;
        public static Dictionary<(string beam, int span), int> CappedCapacities => _cappedCapacityLookup.Value;


        /// <summary>
        /// Get interpolated load capacity for a specific beam and span, handling odd-length spans
        /// FIXED: Better error handling and validation
        /// </summary>
        /// <param name="beamDesignation">Beam designation</param>
        /// <param name="spanLength">Span length in feet (can be decimal)</param>
        /// <param name="capped">Whether to use capped or uncapped capacity tables</param>
        /// <returns>Interpolated load capacity in pounds, or 0 if not found</returns>
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

            var lookup = capped ? CappedCapacities : UncappedCapacities;
            var availableSpans = capped ? CappedCapacityData.AvailableSpans : UncappedCapacityData.AvailableSpans;

            // Check if beam exists in our data at all
            bool beamExists = lookup.Keys.Any(k => k.beam == beamDesignation);
            if (!beamExists)
            {
                Console.WriteLine($"ERROR: Beam {beamDesignation} not found in {(capped ? "capped" : "uncapped")} capacity data");
                return 0;
            }

            // For exact integer spans, try direct lookup first
            if (Math.Abs(spanLength - Math.Round(spanLength)) < 0.001)
            {
                int exactSpan = (int)Math.Round(spanLength);
                if (lookup.TryGetValue((beamDesignation, exactSpan), out var exactCapacity))
                {
                    Console.WriteLine($"DEBUG: Found exact capacity {exactCapacity} lbs for {beamDesignation} at {exactSpan} ft");
                    return exactCapacity;
                }
            }

            // Find available spans for this specific beam
            var beamSpans = lookup.Keys
                .Where(k => k.beam == beamDesignation)
                .Select(k => k.span)
                .OrderBy(s => s)
                .ToList();

            if (beamSpans.Count == 0)
            {
                Console.WriteLine($"ERROR: No span data found for beam {beamDesignation}");
                return 0;
            }

            // Check if span is within available range
            if (spanLength < beamSpans.Min())
            {
                Console.WriteLine($"WARNING: Span {spanLength} ft is below minimum available span {beamSpans.Min()} ft for {beamDesignation}");
                return 0; // Don't extrapolate below minimum
            }

            if (spanLength > beamSpans.Max())
            {
                Console.WriteLine($"WARNING: Span {spanLength} ft is above maximum available span {beamSpans.Max()} ft for {beamDesignation}");
                return 0; // Don't extrapolate above maximum
            }

            // Find the nearest spans below and above for interpolation
            var lowerSpan = beamSpans.Where(s => s <= spanLength).DefaultIfEmpty(0).Max();
            var upperSpan = beamSpans.Where(s => s >= spanLength).DefaultIfEmpty(0).Min();

            // If we have exact match, return it
            if (lowerSpan == upperSpan)
            {
                var exactCapacity = lookup[(beamDesignation, lowerSpan)];
                Console.WriteLine($"DEBUG: Exact match - {beamDesignation} at {lowerSpan} ft = {exactCapacity} lbs");
                return exactCapacity;
            }

            // Get capacities for interpolation
            var lowerCapacity = lookup[(beamDesignation, lowerSpan)];
            var upperCapacity = lookup[(beamDesignation, upperSpan)];

            // Perform linear interpolation
            double interpolationFactor = (spanLength - lowerSpan) / (upperSpan - lowerSpan);
            double interpolatedCapacity = lowerCapacity + interpolationFactor * (upperCapacity - lowerCapacity);

            Console.WriteLine($"DEBUG: Interpolated {beamDesignation} at {spanLength:F1} ft: " +
                            $"Between {lowerSpan}ft({lowerCapacity} lbs) and {upperSpan}ft({upperCapacity} lbs) = {interpolatedCapacity:F0} lbs");

            return interpolatedCapacity;
        }

        /// <summary>
        /// Find the lightest adequate beam for given load and span, supporting interpolation
        /// FIXED: Now sorts by weight only (not utilization) when span is specified
        /// </summary>
        /// <param name="requiredCapacity">Required load capacity in pounds</param>
        /// <param name="spanLength">Span length in feet (can be decimal)</param>
        /// <param name="capped">Whether to use capped beam system</param>
        /// <param name="topN">Number of top candidates to return</param>
        /// <returns>List of lightest adequate beams, or empty list if none found</returns>
        public static List<BeamProperties> FindTopAdequateBeams(double requiredCapacity, double spanLength, bool capped = false, int topN = 5)
        {
            Console.WriteLine($"DEBUG: FindTopAdequateBeams called with ECL = {requiredCapacity:F0} lbs, span = {spanLength:F1} ft, capped = {capped}");

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

            // Ensure span is within reasonable engineering limits
            if (spanLength > 120 || spanLength < 5)
            {
                Console.WriteLine($"WARNING: Span {spanLength} ft is outside typical engineering range (5-120 ft)");
            }

            var beams = capped ? CappedBeams.Values : UncappedBeams.Values;
            var candidates = new List<(BeamProperties beam, double capacity, double utilization)>();

            // Check each beam's adequacy using interpolated capacity
            foreach (var beam in beams)
            {
                double beamCapacity = GetInterpolatedLoadCapacity(beam.Designation, spanLength, capped);

                if (beamCapacity > 0 && beamCapacity >= requiredCapacity)
                {
                    double utilization = (requiredCapacity / beamCapacity) * 100.0;
                    candidates.Add((beam, beamCapacity, utilization));

                    Console.WriteLine($"DEBUG: {beam.Designation} - Capacity: {beamCapacity:F0} lbs, Utilization: {utilization:F1}%");
                }
            }

            if (candidates.Count == 0)
            {
                Console.WriteLine($"ERROR: No adequate beams found for ECL = {requiredCapacity:F0} lbs at span = {spanLength:F1} ft");

                // Show some available beams for debugging
                Console.WriteLine("DEBUG: Available beams and their capacities:");
                var debugBeams = beams.Take(5).ToList();
                foreach (var beam in debugBeams)
                {
                    double debugCapacity = GetInterpolatedLoadCapacity(beam.Designation, spanLength, capped);
                    Console.WriteLine($"  {beam.Designation}: {debugCapacity:F0} lbs (required: {requiredCapacity:F0} lbs)");
                }

                return new List<BeamProperties>();
            }

            // FIXED: Sort by weight ONLY when span is specified
            // Removed the .ThenBy(c => c.utilization) that was causing the issue
            var sortedCandidates = candidates
                .OrderBy(c => c.beam.Weight)  // Sort by weight only - lightest first
                .Take(topN)
                .ToList();

            Console.WriteLine($"SUCCESS: Found {sortedCandidates.Count} adequate beams for ECL = {requiredCapacity:F0} lbs at span = {spanLength:F1} ft");
            Console.WriteLine($"FIXED: Sorted by weight only (not utilization)");

            // Log the top candidates with table validation
            for (int i = 0; i < sortedCandidates.Count; i++)
            {
                var candidate = sortedCandidates[i];
                var tableInfo = GetTableValidationInfo(candidate.beam.Designation, spanLength, capped);

                Console.WriteLine($"  {i + 1}. {candidate.beam.Designation} - {candidate.beam.Weight:F1} lbs/ft");
                Console.WriteLine($"     Table: {tableInfo}");
                Console.WriteLine($"     Result: {candidate.capacity:F0} lbs capacity, {candidate.utilization:F1}% utilization");
            }

            return sortedCandidates.Select(c => c.beam).ToList();
        }

        /// <summary>
        /// Get K-factors based on wheelbase to span ratio
        /// </summary>
        /// <param name="wheelbaseToSpanRatio">Ratio of wheelbase (A) to support centers (L)</param>
        /// <returns>Tuple of (k1, k2) factors</returns>
        public static (double k1, double k2) GetKFactors(double wheelbaseToSpanRatio)
        {
            return KFactorData.GetKFactors(wheelbaseToSpanRatio);
        }

        /// <summary>
        /// Get table validation information for engineering review
        /// Shows exactly what table values are being used in calculations
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
                var beamSpans = lookup.Keys
                    .Where(k => k.beam == beamDesignation)
                    .Select(k => k.span)
                    .OrderBy(s => s)
                    .ToList();

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



        private static Dictionary<(string, int), int> BuildUncappedCapacityLookup()
        {
            var lookup = new Dictionary<(string, int), int>();

            // Load from UncappedCapacityData
            foreach (var beamCapacities in UncappedCapacityData.LoadCapacities)
            {
                foreach (var spanCapacity in beamCapacities.Value)
                {
                    lookup[(beamCapacities.Key, spanCapacity.Key)] = spanCapacity.Value;
                }
            }

            Console.WriteLine($"Built uncapped capacity lookup with {lookup.Count} entries");
            return lookup;
        }

        private static Dictionary<(string, int), int> BuildCappedCapacityLookup()
        {
            var lookup = new Dictionary<(string, int), int>();

            // Load from CappedCapacityData
            foreach (var beamCapacities in CappedCapacityData.LoadCapacities)
            {
                foreach (var spanCapacity in beamCapacities.Value)
                {
                    lookup[(beamCapacities.Key, spanCapacity.Key)] = spanCapacity.Value;
                }
            }

            Console.WriteLine($"Built capped capacity lookup with {lookup.Count} entries");
            return lookup;
        }
    }
}