// UncappedCapacityData.cs
// Load capacity data for uncapped beams from P13.xlsx
// Values represent maximum allowable Equivalent Concentrated Load (ECL) in pounds

using System;
using System.Collections.Generic;
using System.Linq;

namespace BeamSizing.Data
{
    public static class UncappedCapacityData
    {
        /// <summary>
        /// Load capacity lookup table: [BeamDesignation][SpanLength] = Capacity
        /// Capacity values in pounds (lbs)
        /// Suffixes indicate controlling limit:
        /// - C: Compression flange buckling
        /// - D: Deflection (L/240)
        /// - T: Tension limit
        /// - No suffix: General allowable load
        /// </summary>
        public static readonly Dictionary<string, Dictionary<int, int>> LoadCapacities = 
            new Dictionary<string, Dictionary<int, int>>
        {
            ["W6x9"] = new Dictionary<int, int>
            {
                [10] = 1996, [12] = 1357, [14] = 968
            },
            
            ["W6x15"] = new Dictionary<int, int>
            {
                [10] = 3547, [12] = 2416, [14] = 1726
            },
            
            ["W8x10"] = new Dictionary<int, int>
            {
                [10] = 2616, [12] = 1791, [14] = 1290, [16] = 961, [18] = 733
            },
            
            ["W8x18"] = new Dictionary<int, int>
            {
                [10] = 7632, [12] = 5243, [14] = 3794, [16] = 2845, [18] = 2188
            },
            
            ["W8x24"] = new Dictionary<int, int>
            {
                [10] = 10210, [12] = 7014, [14] = 5076, [16] = 3807, [18] = 2928
            },

            ["W8x31"] = new Dictionary<int, int>
            {
                [10] = 13570, [12] = 9325, [14] = 6751, [16] = 5066, [18] = 3899
            },

            ["W10x12"] = new Dictionary<int, int>
            {
                [10] = 3003, [12] = 2055, [14] = 1479, [16] = 1100, [18] = 837, [20] = 646, [22] = 501
            },

            ["W10x22"] = new Dictionary<int, int>
            {
                [10] = 13499, [12] = 10088, [14] = 7340, [16] = 5547, [18] = 4309, [20] = 3416, [22] = 2748
            },

            ["W10x33"] = new Dictionary<int, int>
            {
                [10] = 20366, [12] = 14524, [14] = 10564, [16] = 7979, [18] = 6194, [20] = 4905, [22] = 3941
            },

            ["W10x49"] = new Dictionary<int, int>
            {
                [10] = 31783, [12] = 23266, [14] = 16935, [16] = 12804, [18] = 9953, [20] = 7896, [22] = 6358
            },

            ["W12x14"] = new Dictionary<int, int>
            {
                [10] = 3654, [12] = 2502, [14] = 1802, [16] = 1343, [18] = 1023, [20] = 791, [22] = 615, [24] = 478, [26] = 369
            },

            ["W12x26"] = new Dictionary<int, int>
            {
                [10] = 19462, [12] = 15446, [14] = 11281, [16] = 8568, [18] = 6700, [20] = 5357, [22] = 4356, [24] = 3588, [26] = 2985
            },

            ["W12x40"] = new Dictionary<int, int>
            {
                [10] = 30244, [12] = 25130, [14] = 19439, [16] = 14751, [18] = 11521, [20] = 9197, [22] = 7464, [24] = 6134, [26] = 5088
            },

            ["W12x53"] = new Dictionary<int, int>
            {
                [10] = 41149, [12] = 34194, [14] = 26667, [16] = 20242, [18] = 15816, [20] = 12632, [22] = 10258, [24] = 8437, [26] = 7005
            },

            ["W12x65"] = new Dictionary<int, int>
            {
                [10] = 51237, [12] = 42578, [14] = 33456, [16] = 25400, [18] = 19852, [20] = 15860, [22] = 12885, [24] = 10603, [26] = 8809
            },

            ["W14x22"] = new Dictionary<int, int>
            {
                [10] = 11674, [12] = 8052, [14] = 5858, [16] = 4427, [18] = 3439, [20] = 2726, [22] = 2193, [24] = 1782, [26] = 1457, [28] = 1195, [30] = 979
            },

            ["W14x30"] = new Dictionary<int, int>
            {
                [10] = 24487, [12] = 18021, [14] = 13162, [16] = 9998, [18] = 7819, [20] = 6252, [22] = 5085, [24] = 4190, [26] = 3487, [28] = 2923, [30] = 2462
            },

            ["W14x43"] = new Dictionary<int, int>
            {
                [10] = 36565, [12] = 30392, [14] = 25970, [16] = 20488, [18] = 16044, [20] = 12850, [22] = 10473, [24] = 8652, [26] = 7223, [28] = 6078, [30] = 5144
            },

            ["W14x61"] = new Dictionary<int, int>
            {
                [10] = 53779, [12] = 44704, [14] = 38205, [16] = 30670, [18] = 24029, [20] = 19257, [22] = 15706, [24] = 12987, [26] = 10854, [28] = 9146, [30] = 7754
            },

            ["W14x82"] = new Dictionary<int, int>
            {
                [10] = 71742, [12] = 59635, [14] = 50963, [16] = 42288, [18] = 33138, [20] = 26564, [22] = 21673, [24] = 17929, [26] = 14992, [28] = 12641, [30] = 10724
            },

            ["W14x90"] = new Dictionary<int, int>
            {
                [10] = 83434, [12] = 69363, [14] = 59287, [16] = 47926, [18] = 37566, [20] = 30124, [22] = 24588, [24] = 20351, [26] = 17028, [28] = 14368, [30] = 11202
            },

            ["W16x26"] = new Dictionary<int, int>
            {
                [10] = 15345, [12] = 10590, [14] = 7713, [16] = 5837, [18] = 4542, [20] = 3609, [22] = 2911, [24] = 2375, [26] = 1951, [28] = 1610, [30] = 1329, [32] = 1095, [34] = 897
            },

            ["W16x36"] = new Dictionary<int, int>
            {
                [10] = 32963, [12] = 24556, [14] = 17948, [16] = 13646, [18] = 10686, [20] = 8558, [22] = 6974, [24] = 5761, [26] = 4809, [28] = 4046, [30] = 3423, [32] = 2907, [34] = 2474
            },

            ["W16x57"] = new Dictionary<int, int>
            {
                [10] = 53800, [12] = 44728, [14] = 38233, [16] = 33347, [18] = 28631, [20] = 22998, [22] = 18811, [24] = 15610, [26] = 13103, [28] = 11099, [30] = 9469, [32] = 8122, [34] = 6993
            },

            ["W16x89"] = new Dictionary<int, int>
            {
                [10] = 90478, [12] = 75235, [14] = 64322, [16] = 56115, [18] = 49201, [20] = 39552, [22] = 32383, [24] = 26904, [26] = 22615, [28] = 19189, [30] = 16404, [32] = 14104, [34] = 12179
            },

            ["W18x35"] = new Dictionary<int, int>
            {
                [10] = 27486, [12] = 18999, [14] = 13868, [16] = 10525, [18] = 8222, [20] = 6565, [22] = 5330, [24] = 4382, [26] = 3637, [28] = 3038, [30] = 2548, [32] = 2141, [34] = 1798, [36] = 1504, [38] = 1250
            },

            ["W18x46"] = new Dictionary<int, int>
            {
                [10] = 45994, [12] = 36751, [14] = 26881, [16] = 20460, [18] = 16042, [20] = 12870, [22] = 10510, [24] = 8705, [26] = 7289, [28] = 6157, [30] = 5234, [32] = 4471, [34] = 3830, [36] = 3286, [38] = 2818
            },

            ["W18x65"] = new Dictionary<int, int>
            {
                [10] = 68307, [12] = 56804, [14] = 48568, [16] = 42375, [18] = 36753, [20] = 29594, [22] = 24280, [24] = 20222, [26] = 17050, [28] = 14520, [30] = 12466, [32] = 10774, [34] = 9360, [36] = 8164, [38] = 7142
            },

            ["W18x97"] = new Dictionary<int, int>
            {
                [12] = 91319, [14] = 78093, [16] = 68150, [18] = 60394, [20] = 53528, [22] = 43906, [24] = 36559, [26] = 30814, [28] = 26231, [30] = 22510, [32] = 19443, [34] = 16880, [36] = 14712, [38] = 12859
            },

            ["W21x44"] = new Dictionary<int, int>
            {
                [10] = 38289, [12] = 26478, [14] = 19340, [16] = 14691, [18] = 11490, [20] = 9187, [22] = 7472, [24] = 6158, [26] = 5125, [28] = 4296, [30] = 3619, [32] = 3056, [34] = 2583, [36] = 2179, [38] = 1831, [40] = 1527, [42] = 1259, [44] = 1021
            },

            ["W21x57"] = new Dictionary<int, int>
            {
                [10] = 64828, [12] = 51645, [14] = 37795, [16] = 28786, [18] = 22592, [20] = 18145, [22] = 14840, [24] = 12312, [26] = 10333, [28] = 8750, [30] = 7463, [32] = 6398, [34] = 5507, [36] = 4750, [38] = 4101, [40] = 3539, [42] = 3047, [44] = 2612
            },

            ["W21x83"] = new Dictionary<int, int>
            {
                [10] = 99894, [12] = 83092, [14] = 71068, [16] = 62029, [18] = 54980, [20] = 45557, [22] = 37423, [24] = 31217, [26] = 26369, [28] = 22505, [30] = 19371, [32] = 16792, [34] = 14640, [36] = 12823, [38] = 11272, [40] = 9937, [42] = 8775, [44] = 7758
            },

            ["W21x111"] = new Dictionary<int, int>
            {
                [16] = 90402, [18] = 80147, [20] = 71922, [22] = 65171, [24] = 56333, [26] = 47615, [28] = 40668, [30] = 35037, [32] = 30404, [34] = 26540, [36] = 23279, [38] = 20499, [40] = 18104, [42] = 16024, [44] = 14203
            },

            ["W24x55"] = new Dictionary<int, int>
            {
                [10] = 56765, [12] = 39281, [14] = 28717, [16] = 21841, [18] = 17110, [20] = 13710, [22] = 11180, [24] = 9243, [26] = 7723, [28] = 6505, [30] = 5513, [32] = 4690, [34] = 3999, [36] = 3411, [38] = 2905, [40] = 2465, [42] = 2078, [44] = 1736, [46] = 1430, [48] = 1155, [50] = 906
            },

            ["W24x68"] = new Dictionary<int, int>
            {
                [10] = 89996, [12] = 74872, [14] = 57412, [16] = 43777, [18] = 34407, [20] = 27685, [22] = 22694, [24] = 18882, [26] = 15900, [28] = 13520, [30] = 11587, [32] = 9992, [34] = 8659, [36] = 7530, [38] = 6565, [40] = 5731, [42] = 5004, [44] = 4364, [46] = 3798, [48] = 3292, [50] = 2838
            },

            ["W24x76"] = new Dictionary<int, int>
            {
                [12] = 85578, [14] = 73212, [16] = 57958, [18] = 45590, [20] = 36722, [22] = 30141, [24] = 25117, [26] = 21191, [28] = 18059, [30] = 15518, [32] = 13425, [34] = 11677, [36] = 10200, [38] = 8939, [40] = 7850, [42] = 6903, [44] = 6072, [46] = 5337, [48] = 4683, [50] = 4097
            },

            ["W24x104"] = new Dictionary<int, int>
            {
                [16] = 93757, [18] = 83143, [20] = 74631, [22] = 67648, [24] = 58089, [26] = 49207, [28] = 42139, [30] = 36416, [32] = 31713, [34] = 27798, [36] = 24500, [38] = 21693, [40] = 19281, [42] = 17191, [44] = 15366, [46] = 13760, [48] = 12338, [50] = 11071
            },

            ["W27x84"] = new Dictionary<int, int>
            {
                [14] = 85857, [16] = 65512, [18] = 51538, [20] = 41518, [22] = 34082, [24] = 28407, [26] = 23972, [28] = 20435, [30] = 17566, [32] = 15202, [34] = 13229, [36] = 11561, [38] = 10137, [40] = 8909, [42] = 7841, [44] = 6903, [46] = 6075, [48] = 5338, [50] = 4677, [52] = 4082, [54] = 3542, [56] = 3050
            },

            ["W27x94"] = new Dictionary<int, int>
            {
                [16] = 86730, [18] = 68275, [20] = 55048, [22] = 45237, [24] = 37753, [26] = 31907, [28] = 27249, [30] = 23473, [32] = 20366, [34] = 17775, [36] = 15588, [38] = 13723, [40] = 12117, [42] = 10722, [44] = 9500, [46] = 8422, [48] = 7464, [50] = 6608, [52] = 5838, [54] = 5142, [56] = 4509 
            },

            ["W30x99"] = new Dictionary<int, int>
            {
                [16] = 81924, [18] = 64464, [20] = 51948, [22] = 42661, [24] = 35574, [26] = 30037, [28] = 25623, [30] = 22043, [32] = 19095, [34] = 16634, [36] = 14557, [38] = 12783, [40] = 11254, [42] = 9925, [44] = 8759, [46] = 7730, [48] = 6814, [50] = 5995, [52] = 5257, [54] = 4588, [56] = 3980, [58] = 3423, [60] = 2912
            },

            ["W30x108"] = new Dictionary<int, int>
            {
                [18] = 81123, [20] = 65417, [22] = 53768, [24] = 44882, [26] = 37943, [28] = 32415, [30] = 27934, [32] = 24247, [34] = 21173, [36] = 18580, [38] = 16368, [40] = 14464, [42] = 12811, [44] = 11363, [46] = 10086, [48] = 8952, [50] = 7939, [52] = 7029, [54] = 6205, [56] = 5458, [58] = 4775, [60] = 4148
            },

            ["W33x118"] = new Dictionary<int, int>
            {
                [18] = 94420, [20] = 76161, [22] = 62620, [24] = 52293, [26] = 42230, [28] = 37807, [30] = 32603, [32] = 28323, [34] = 24755, [36] = 21746, [38] = 19182, [40] = 16875, [42] = 15059, [44] = 13383, [46] = 11906, [48] = 10595, [50] = 9424, [52] = 8373, [54] = 7423, [56] = 6561, [58] = 5774, [60] = 5053
            },

            ["W36x135"] = new Dictionary<int, int>
            {
                [20] = 95794, [22] = 78799, [24] = 65841, [26] = 55726, [28] = 47673, [30] = 41150, [32] = 35787, [34] = 31319, [36] = 27552, [38] = 24344, [40] = 21586, [42] = 19193, [44] = 17101, [46] = 15258, [48] = 13625, [50] = 12168, [52] = 10860, [54] = 9680, [56] = 8611, [58] = 7636, [60] = 6743
            },

            ["W36x150"] = new Dictionary<int, int>
            {
                    [24] = 89776, [26] = 76079, [28] = 65180, [30] = 56358, [32] = 49111, [34] = 43079, [36] = 38000, [38] = 33679, [40] = 29967, [42] = 26752, [44] = 23946, [46] = 21478, [48] = 19294, [50] = 17349, [52] = 15607, [54] = 14039, [56] = 12620, [58] = 11330, [60] = 10152
                },

                ["W36x230"] = new Dictionary<int, int>
                {
                    [40] = 96197, [42] = 86596, [44] = 78243, [46] = 70927, [48] = 64478, [50] = 58760, [52] = 53663, [54] = 49097, [56] = 44987, [58] = 41271, [60] = 37899
                }
            };

        /// <summary>
        /// Lower flange loading values for each beam (from column B in Excel)
        /// These represent the maximum load that can be applied to the lower flange
        /// </summary>
        public static readonly Dictionary<string, int> LowerFlangeLoading =
            new Dictionary<string, int>
            {
                ["W6x9"] = 490,
                ["W6x15"] = 720,
                ["W8x10"] = 450,
                ["W8x18"] = 1160,
                ["W8x24"] = 1705,
                ["W8x31"] = 2020,
                ["W10x12"] = 470,
                ["W10x22"] = 1380,
                ["W10x33"] = 2020,
                ["W10x49"] = 3345,
                ["W12x14"] = 540,
                ["W12x26"] = 1540,
                ["W12x40"] = 2830,
                ["W12x53"] = 3525,
                ["W12x65"] = 3905,
                ["W14x22"] = 1200,
                ["W14x30"] = 1580,
                ["W14x43"] = 2995,
                ["W14x61"] = 4440,
                ["W14x82"] = 7800,
                ["W14x90"] = 5375,
                ["W16x26"] = 1270,
                ["W16x36"] = 1970,
                ["W16x57"] = 5450,
                ["W16x89"] = 8165,
                ["W18x35"] = 1925,
                ["W18x46"] = 3905,
                ["W18x65"] = 6000,
                ["W18x97"] = 8070,
                ["W21x44"] = 2160,
                ["W21x57"] = 4505,
                ["W21x83"] = 7435,
                ["W21x111"] = 8165,
                ["W24x55"] = 2720,
                ["W24x68"] = 3650,
                ["W24x76"] = 4930,
                ["W24x104"] = 6000,
                ["W27x84"] = 4370,
                ["W27x94"] = 5920,
                ["W30x99"] = 4790,
                ["W30x108"] = 6160,
                ["W33x118"] = 5840,
                ["W36x135"] = 6655,
                ["W36x150"] = 9425,
                ["W36x230"] = 16930
            };

        /// <summary>
        /// Get all available span lengths for capacity lookups
        /// </summary>
        public static readonly int[] AvailableSpans = { 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60 };

        /// <summary>
        /// Get load capacity for a specific beam and span
        /// </summary>
        /// <param name="beamDesignation">Beam designation (e.g., "W12x40")</param>
        /// <param name="spanLength">Span length in feet</param>
        /// <returns>Load capacity in pounds, or 0 if not found</returns>
        public static int GetCapacity(string beamDesignation, int spanLength)
        {
            if (LoadCapacities.TryGetValue(beamDesignation, out var spanCapacities))
            {
                return spanCapacities.TryGetValue(spanLength, out var capacity) ? capacity : 0;
            }
            return 0;
        }

        /// <summary>
        /// Get lower flange loading for a specific beam
        /// </summary>
        /// <param name="beamDesignation">Beam designation</param>
        /// <returns>Lower flange loading in pounds, or 0 if not found</returns>
        public static int GetLowerFlangeLoading(string beamDesignation)
        {
            return LowerFlangeLoading.TryGetValue(beamDesignation, out var loading) ? loading : 0;
        }

        /// <summary>
        /// Get all beams that can support a given load at a specific span
        /// </summary>
        /// <param name="requiredCapacity">Required load capacity in pounds</param>
        /// <param name="spanLength">Span length in feet</param>
        /// <returns>List of adequate beam designations, ordered by capacity</returns>
        public static List<string> GetAdequateBeams(double requiredCapacity, int spanLength)
        {
            var adequateBeams = new List<string>();

            foreach (var beamCapacities in LoadCapacities)
            {
                if (beamCapacities.Value.TryGetValue(spanLength, out var capacity) && capacity >= requiredCapacity)
                {
                    adequateBeams.Add(beamCapacities.Key);
                }
            }

            return adequateBeams;
        }

        /// <summary>
        /// Validates that the capacity data is complete and consistent
        /// </summary>
        /// <returns>True if data validation passes</returns>
        public static bool ValidateData()
        {
            var beamCount = LoadCapacities.Count;
            var lflCount = LowerFlangeLoading.Count;

            if (beamCount != lflCount)
            {
                Console.WriteLine($"Data validation failed: {beamCount} beams with capacities but {lflCount} with LFL values");
                return false;
            }

            // Check that all beams in LoadCapacities have LFL values
            foreach (var beam in LoadCapacities.Keys)
            {
                if (!LowerFlangeLoading.ContainsKey(beam))
                {
                    Console.WriteLine($"Data validation failed: {beam} missing LFL value");
                    return false;
                }
            }

            Console.WriteLine($"Uncapped capacity data validation passed: {beamCount} beams loaded successfully");
            return true;
        }
    }
}