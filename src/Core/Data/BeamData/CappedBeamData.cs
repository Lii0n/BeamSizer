// CappedBeamData.cs
// Capped beam properties from P16.xlsx (W-shape + Channel combinations)

using System;
using System.Collections.Generic;
using System.Linq;

namespace BeamSizing.Data
{
    /// <summary>
    /// Extended beam properties class for capped beams (W-shape + Channel)
    /// </summary>
    public class CappedBeamProperties : BeamSizing.BeamProperties
    {
        public string Channel { get; set; } = string.Empty;         // Channel designation (e.g., "10x15.3")
        public double TotalWeight { get; set; }      // Combined weight [lbs/ft]
        public double TotalArea { get; set; }        // Combined area [in²]
        public double Width { get; set; }            // Overall width [in]
        public double Yc { get; set; }               // Distance to compression centroid [in]
        public double Yt { get; set; }               // Distance to tension centroid [in]
        public double ScUpper { get; set; }          // Upper section modulus [in³]
        public double SlLower { get; set; }          // Lower section modulus [in³]
        public double TorsionalConstant { get; set; } // Torsional constant [in⁴]

        // Default constructor
        public CappedBeamProperties() { }

        // Constructor for easy data initialization
        public CappedBeamProperties(
            string wShape,
            string channel,
            double totalWeight,
            double totalArea,
            double width,
            double depth,
            double yc,
            double yt,
            double momentOfInertia,
            double scUpper,
            double slLower,
            double torsionalConstant,
            double sectionModulus)
        {
            Designation = $"{wShape}+{channel}";
            Channel = channel;
            TotalWeight = totalWeight;
            Weight = totalWeight; // For compatibility with base class
            TotalArea = totalArea;
            Area = totalArea; // For compatibility with base class
            Width = width;
            Depth = depth;
            Yc = yc;
            Yt = yt;
            I = momentOfInertia;
            ScUpper = scUpper;
            SlLower = slLower;
            TorsionalConstant = torsionalConstant;
            S = sectionModulus;
        }

        public override string ToString()
        {
            return $"{Designation}: {TotalWeight} lbs/ft, I={I:F1} in⁴, S={S:F1} in³";
        }
    }

    public static class CappedBeamData
    {
        /// <summary>
        /// Complete capped beam properties from P16.xlsx
        /// Each entry represents a W-shape + Channel combination
        /// </summary>
        public static readonly List<CappedBeamProperties> CappedBeams = new List<CappedBeamProperties>
        {
            new CappedBeamProperties("8x18", "10x15.3", 33.3, 9.75, 10, 8.38, 2.617, 5.763, 96.91, 37.03, 16.82, 75.37, 15.07),
            new CappedBeamProperties("8x24", "10x15.3", 39.3, 11.57, 10, 8.17, 2.819, 5.351, 120.1, 42.61, 22.45, 85.7, 17.14),
            new CappedBeamProperties("8x31", "10x15.3", 46.3, 13.62, 10, 8.24, 3.051, 5.189, 151.4, 49.63, 29.18, 104.5, 20.9),

            new CappedBeamProperties("10x22", "10x15.3", 37.3, 10.98, 10, 10.41, 3.407, 7.003, 178.7, 52.44, 25.51, 78.8, 15.76),
            new CappedBeamProperties("10x22", "12x20.7", 42.7, 12.58, 12, 10.452, 3.107, 7.345, 190.4, 61.27, 25.92, 140.4, 23.4),

            new CappedBeamProperties("10x33", "10x15.3", 46.3, 14.2, 10, 9.97, 3.691, 6.279, 233.6, 63.3, 37.21, 104, 20.8),
            new CappedBeamProperties("10x33", "12x20.7", 53.7, 15.8, 12, 10.012, 3.432, 6.58, 247.9, 72.25, 37.68, 165.6, 27.6),

            new CappedBeamProperties("12x26", "10x15.3", 41.3, 12.14, 10, 12.46, 4.236, 8.224, 298.7, 70.52, 36.32, 84.7, 16.94),
            new CappedBeamProperties("12x26", "12x20.7", 46.7, 13.74, 12, 12.502, 3.868, 8.634, 317.8, 82.16, 36.81, 146.3, 24.38),

            new CappedBeamProperties("12x40", "10x15.3", 55.3, 16.29, 10, 12.18, 4.673, 7.507, 413.4, 88.46, 55.07, 111.5, 22.3),
            new CappedBeamProperties("12x40", "12x20.7", 60.7, 17.89, 12, 12.222, 4.361, 7.861, 437.8, 100.4, 55.69, 173.1, 28.85),

            new CappedBeamProperties("14x30", "10x15.3", 45.3, 13.34, 10, 14.08, 4.963, 9.117, 420.1, 84.65, 46.08, 87, 17.4),
            new CappedBeamProperties("14x30", "12x20.7", 50.7, 14.94, 12, 14.122, 4.551, 9.571, 447.5, 98.33, 46.75, 148.6, 24.77),
            new CappedBeamProperties("14x30", "15x33.9", 63.9, 18.78, 15, 14.162, 4.162, 10, 468.7, 112.6, 49.87, 241.1, 33.12),

            new CappedBeamProperties("14x43", "12x20.7", 63.7, 18.5, 12, 13.66, 4.821, 8.839, 608.6, 126.3, 68.9, 148.6, 31.4),
            new CappedBeamProperties("14x43", "15x33.9", 76.9, 22.54, 15, 13.7, 4.44, 9.26, 638.7, 143.8, 72.2, 241.1, 36.93),

            new CappedBeamProperties("14x61", "15x33.9", 94.9, 27.86, 15, 13.89, 4.831, 9.059, 870.4, 180.2, 96.1, 241.1, 46.66),

            new CappedBeamProperties("14x82", "15x33.9", 115.9, 34.06, 15, 14.31, 5.307, 9.003, 1139.4, 214.7, 126.5, 241.1, 55.77),

            new CappedBeamProperties("16x36", "10x15.3", 51.3, 15.1, 10, 15.86, 5.717, 10.143, 593.6, 103.8, 58.5, 87, 21.89),
            new CappedBeamProperties("16x36", "12x20.7", 56.7, 16.7, 12, 15.86, 5.275, 10.585, 628.7, 119.2, 62, 148.6, 27.16),
            new CappedBeamProperties("16x36", "15x33.9", 69.9, 20.54, 15, 15.86, 4.886, 10.974, 668.3, 136.8, 65.9, 241.1, 32.54),

            new CappedBeamProperties("16x57", "12x20.7", 77.7, 22.76, 12, 16.43, 5.915, 10.515, 983.5, 166.3, 93.5, 148.6, 37.56),
            new CappedBeamProperties("16x57", "15x33.9", 90.9, 26.6, 15, 16.43, 5.526, 10.904, 1032.9, 186.9, 97.3, 241.1, 42.93),

            new CappedBeamProperties("18x35", "10x15.3", 50.3, 14.8, 10, 17.7, 6.367, 11.333, 678.5, 106.6, 59.9, 87, 22.36),
            new CappedBeamProperties("18x35", "12x20.7", 55.7, 16.4, 12, 17.7, 5.925, 11.775, 720.3, 121.6, 63.6, 148.6, 27.63),
            new CappedBeamProperties("18x35", "15x33.9", 68.9, 20.24, 15, 17.7, 5.536, 12.164, 766.5, 138.5, 67.7, 241.1, 33.01),

            new CappedBeamProperties("18x46", "12x20.7", 66.7, 19.6, 12, 18.06, 6.325, 11.735, 954.9, 150.8, 81.4, 148.6, 33.97),
            new CappedBeamProperties("18x46", "15x33.9", 79.9, 23.44, 15, 18.06, 5.936, 12.124, 1009.4, 170.1, 85.7, 241.1, 39.34),

            new CappedBeamProperties("18x65", "15x33.9", 98.9, 29.06, 15, 18.35, 6.436, 11.914, 1374.7, 213.6, 115.4, 241.1, 49.86),

            new CappedBeamProperties("21x44", "12x20.7", 64.7, 19.0, 12, 20.66, 7.333, 13.327, 1218.8, 166.3, 91.4, 148.6, 36.43),
            new CappedBeamProperties("21x44", "15x33.9", 77.9, 22.84, 15, 20.66, 6.944, 13.716, 1282.4, 184.7, 95.9, 241.1, 41.8),

            new CappedBeamProperties("21x57", "15x33.9", 90.9, 26.64, 15, 21.06, 7.394, 13.666, 1647.8, 222.9, 122.3, 241.1, 48.26),

            new CappedBeamProperties("24x55", "12x20.7", 75.7, 22.24, 12, 23.57, 8.617, 14.953, 1967.7, 228.4, 131.6, 148.6, 46.52),
            new CappedBeamProperties("24x55", "15x33.9", 88.9, 26.08, 15, 23.57, 8.228, 15.342, 2049.5, 249.1, 136.9, 241.1, 51.89),

            new CappedBeamProperties("24x68", "15x33.9", 101.9, 29.96, 15, 23.73, 8.678, 15.052, 2500.2, 288.1, 166.1, 241.1, 58.94),

            new CappedBeamProperties("27x84", "15x33.9", 117.9, 34.66, 15, 26.71, 9.627, 17.083, 3550.2, 368.8, 207.9, 241.1, 71.88),

            new CappedBeamProperties("30x99", "15x33.9", 132.9, 39.04, 15, 29.65, 10.617, 19.033, 4850.2, 456.9, 254.8, 241.1, 87.12),

            new CappedBeamProperties("33x118", "15x33.9", 151.9, 44.64, 15, 32.86, 11.617, 21.243, 6770.2, 582.9, 322.8, 241.1, 108.59),

            new CappedBeamProperties("36x135", "15x33.9", 168.9, 49.64, 15, 35.55, 12.617, 22.933, 8970.2, 711.1, 391.1, 241.1, 128.21)
        };

    }
}