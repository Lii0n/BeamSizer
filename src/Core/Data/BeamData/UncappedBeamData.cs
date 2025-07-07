// UncappedBeamData.cs
// Uncapped beam properties from P12.xlsx to keep main code clean

using System.Collections.Generic;

namespace BeamSizing.Data
{
    public static class UncappedBeamData
    {
        /// <summary>
        /// Uncapped beam properties from P12.xlsx
        /// Contains all standard AISC wide-flange beam sections
        /// </summary>
        public static readonly List<BeamSizing.BeamProperties> UncappedBeams = new List<BeamSizing.BeamProperties>
        {
            new BeamSizing.BeamProperties("W6x9", 5.9, 9, 2.68, 0.17, 3.94, 0.215, 0.847, 16.4, 5.56, 2.47, 2.25),
            new BeamSizing.BeamProperties("W6x15", 5.99, 15, 4.43, 0.23, 5.99, 0.26, 1.557, 29.1, 9.72, 2.56, 3.5),
            new BeamSizing.BeamProperties("W8x10", 7.89, 10, 2.96, 0.17, 3.94, 0.205, 0.808, 30.8, 7.81, 3.22, 2.25),
            new BeamSizing.BeamProperties("W8x18", 8.14, 18, 5.26, 0.23, 5.25, 0.33, 1.732, 61.9, 15.2, 3.43, 2.25),
            new BeamSizing.BeamProperties("W8x24", 7.93, 24, 7.08, 0.245, 6.495, 0.4, 2.598, 82.8, 20.9, 3.42, 3.5),
            new BeamSizing.BeamProperties("W8x31", 8, 31, 9.13, 0.285, 7.995, 0.435, 3.478, 110, 27.5, 3.47, 5.5),
            new BeamSizing.BeamProperties("W10x12", 9.87, 12, 3.54, 0.19, 3.96, 0.21, 0.832, 53.8, 10.9, 3.9, 2.25),
            new BeamSizing.BeamProperties("W10x22", 10.17, 22, 6.49, 0.24, 5.75, 0.36, 2.07, 118, 23.2, 4.27, 2.75),
            new BeamSizing.BeamProperties("W10x33", 9.73, 33, 9.71, 0.29, 7.96, 0.435, 3.463, 170, 35, 4.19, 5.5),
            new BeamSizing.BeamProperties("W10x49", 9.98, 49, 14.4, 0.34, 10, 0.56, 5.6, 272, 54.6, 4.35, 5.5),
            new BeamSizing.BeamProperties("W12x14", 11.91, 14, 4.16, 0.2, 3.97, 0.225, 0.893, 88.6, 14.9, 4.62, 2.25),
            new BeamSizing.BeamProperties("W12x26", 12.22, 26, 7.65, 0.23, 6.49, 0.38, 2.466, 204, 33.4, 5.17, 3.5),
            new BeamSizing.BeamProperties("W12x40", 11.94, 40, 11.8, 0.295, 8.005, 0.515, 4.123, 310, 51.9, 5.13, 5.5),
            new BeamSizing.BeamProperties("W12x53", 12.06, 53, 15.6, 0.345, 9.995, 0.55, 5.747, 425, 70.6, 5.23, 5.5),
            new BeamSizing.BeamProperties("W12x65", 12.12, 65, 19.1, 0.39, 12, 0.65, 7.26, 533, 87.9, 5.28, 5.5),
            new BeamSizing.BeamProperties("W14x22", 13.74, 22, 6.4, 0.23, 5, 0.335, 1.675, 199, 29, 5.54, 2.75),
            new BeamSizing.BeamProperties("W14x30", 13.84, 30, 8.85, 0.27, 6.73, 0.385, 2.591, 291, 42, 5.73, 3.5),
            new BeamSizing.BeamProperties("W14x43", 13.66, 43, 12.6, 0.305, 7.995, 0.53, 4.237, 428, 62.7, 5.82, 5.5),
            new BeamSizing.BeamProperties("W14x61", 13.89, 61, 17.9, 0.375, 9.995, 0.645, 6.447, 640, 92.2, 5.98, 5.5),
            new BeamSizing.BeamProperties("W14x82", 14.31, 82, 24.1, 0.51, 10.3, 0.855, 8.661, 882, 123, 6.05, 5.5),
            new BeamSizing.BeamProperties("W14x90", 14.02, 90, 26.5, 0.44, 14.52, 0.71, 10.309, 999, 143, 6.14, 5.5),
            new BeamSizing.BeamProperties("W16x26", 15.69, 26, 7.68, 0.25, 5.5, 0.345, 1.897, 301, 38.4, 6.26, 2.75),
            new BeamSizing.BeamProperties("W16x36", 15.86, 36, 10.6, 0.295, 6.985, 0.43, 3.004, 448, 56.5, 6.51, 3.5),
            new BeamSizing.BeamProperties("W16x57", 16.43, 57, 16.8, 0.43, 7.12, 0.715, 5.084, 758, 92.2, 6.72, 3.5),
            new BeamSizing.BeamProperties("W16x89", 16.75, 89, 26.2, 0.525, 10.365, 0.875, 9.069, 1300, 155, 7.05, 5.5),
            new BeamSizing.BeamProperties("W18x35", 17.7, 35, 10.3, 0.3, 6, 0.425, 2.55, 510, 57.6, 7.04, 3.5),
            new BeamSizing.BeamProperties("W18x46", 18.06, 46, 13.5, 0.36, 6.06, 0.605, 3.666, 712, 78.8, 7.25, 3.5),
            new BeamSizing.BeamProperties("W18x65", 18.35, 65, 19.1, 0.45, 7.59, 0.75, 5.692, 1070, 117, 7.49, 3.5),
            new BeamSizing.BeamProperties("W18x97", 18.59, 97, 28.5, 0.535, 11.146, 0.87, 9.696, 1750, 188, 7.82, 5.5),
            new BeamSizing.BeamProperties("W21x44", 20.66, 44, 13, 0.35, 6.5, 0.45, 2.925, 843, 81.6, 8.06, 3.5),
            new BeamSizing.BeamProperties("W21x57", 21.06, 57, 16.7, 0.405, 6.555, 0.65, 4.261, 1170, 111, 8.36, 3.5),
            new BeamSizing.BeamProperties("W21x83", 21.43, 83, 24.3, 0.515, 8.355, 0.835, 6.976, 1830, 171, 8.67, 5.5),
            new BeamSizing.BeamProperties("W21x111", 21.51, 111, 32.7, 0.55, 12.34, 0.875, 10.798, 2670, 249, 9.05, 5.5),
            new BeamSizing.BeamProperties("W24x55", 23.57, 55, 16.2, 0.395, 7.005, 0.505, 3.538, 1350, 114, 9.11, 3.5),
            new BeamSizing.BeamProperties("W24x68", 23.73, 68, 20.1, 0.415, 8.965, 0.585, 5.245, 1830, 154, 9.55, 5.5),
            new BeamSizing.BeamProperties("W24x76", 23.92, 76, 22.4, 0.44, 8.99, 0.68, 6.113, 2100, 176, 9.69, 5.5),
            new BeamSizing.BeamProperties("W24x104", 24.06, 104, 30.6, 0.5, 12.75, 0.75, 9.562, 3100, 258, 10.1, 5.5),
            new BeamSizing.BeamProperties("W27x84", 26.71, 84, 24.8, 0.46, 9.96, 0.64, 6.374, 2850, 213, 10.7, 5.5),
            new BeamSizing.BeamProperties("W27x94", 26.92, 94, 27.7, 0.49, 9.99, 0.745, 7.443, 3270, 243, 10.9, 5.5),
            new BeamSizing.BeamProperties("W30x99", 29.65, 99, 29.1, 0.52, 10.45, 0.67, 7.002, 3990, 269, 11.7, 5.5),
            new BeamSizing.BeamProperties("W30x108", 29.83, 108, 31, 0.545, 10.475, 0.76, 7.961, 4470, 299, 11.9, 5.5),
            new BeamSizing.BeamProperties("W33x118", 32.86, 118, 34.7, 0.55, 11.48, 0.74, 8.495, 5900, 359, 13, 5.5),
            new BeamSizing.BeamProperties("W36x135", 35.55, 135, 39.7, 0.6, 11.95, 0.79, 9.44, 7800, 439, 14, 5.5),
            new BeamSizing.BeamProperties("W36x150", 35.85, 150, 44.2, 0.625, 11.975, 0.94, 11.256, 9040, 504, 14.3, 5.5),
            new BeamSizing.BeamProperties("W36x230", 35.9, 230, 67.6, 0.76, 16.47, 1.26, 20.752, 15000, 837, 14.9, 5.5)
        };
    }
}