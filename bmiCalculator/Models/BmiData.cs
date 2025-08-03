using System.ComponentModel.DataAnnotations;
namespace bmiCalculator.Models
{
    public class BmiData
    {
        public double BMI { get; set; }
        public double HeightInCm { get; set; }
        public double WeightInKg { get; set; }
        public int Age { get; set; }
        public string Category { get; set; }
        public string HeightUnit { get; set; }
        public string WeightUnit { get; set; }
        public double OriginalWeight { get; set; }
        public double? Feet { get; set; }
        public double? Inches { get; set; }
    }
}
