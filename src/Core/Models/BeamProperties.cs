// BeamProperties.cs
// Cleaned up version with only used properties and methods

using System;

namespace BeamSizing
{
    public class BeamProperties
    {
        // Essential structural properties
        public string Designation { get; set; } = string.Empty;
        public double Depth { get; set; }
        public double Weight { get; set; }
        public double Area { get; set; }
        public double WebThickness { get; set; }
        public double FlangeWidth { get; set; }
        public double FlangeThickness { get; set; }
        public double FlangeArea { get; set; }
        public double I { get; set; }                // Moment of inertia
        public double S { get; set; }                // Section modulus
        public double RadiusOfGyration { get; set; }
        public double FlangeGage { get; set; }

        // Default constructor
        public BeamProperties() { }

        // Constructor for easy data initialization
        public BeamProperties(
            string designation,
            double depth,
            double weight,
            double area,
            double webThickness,
            double flangeWidth,
            double flangeThickness,
            double flangeArea,
            double momentOfInertia,
            double sectionModulus,
            double radiusOfGyration,
            double flangeGage)
        {
            Designation = designation;
            Depth = depth;
            Weight = weight;
            Area = area;
            WebThickness = webThickness;
            FlangeWidth = flangeWidth;
            FlangeThickness = flangeThickness;
            FlangeArea = flangeArea;
            I = momentOfInertia;
            S = sectionModulus;
            RadiusOfGyration = radiusOfGyration;
            FlangeGage = flangeGage;
        }

        /// <summary>
        /// Returns a string representation of the beam properties
        /// </summary>
        public override string ToString()
        {
            return $"{Designation}: {Weight} lbs/ft, I={I} in⁴, S={S} in³";
        }
    }
}