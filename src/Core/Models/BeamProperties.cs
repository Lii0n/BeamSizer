// BeamProperties.cs
// Updated class with constructor for easier data initialization

using System;

namespace BeamSizing
{
    public class BeamProperties
    {
        // Properties
        public string Designation { get; set; } = string.Empty;     
        public double Depth { get; set; }            
        public double Weight { get; set; }           
        public double Area { get; set; }             
        public double WebThickness { get; set; }     
        public double FlangeWidth { get; set; }      
        public double FlangeThickness { get; set; }  
        public double FlangeArea { get; set; }       
        public double I { get; set; }                
        public double S { get; set; }                
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

        /// <summary>
        /// Calculates the beam's plastic section modulus (approximate)
        /// </summary>
        public double PlasticSectionModulus => S * 1.1; // Approximate relationship

        /// <summary>
        /// Calculates the beam's web area
        /// </summary>
        public double WebArea => (Depth - 2 * FlangeThickness) * WebThickness;

        /// <summary>
        /// Checks if this is a compact section (simplified check)
        /// </summary>
        public bool IsCompactSection => (FlangeWidth / (2 * FlangeThickness)) < 8.5;
    }
}