using System.Diagnostics;
using bmiCalculator.Models;
using bmiCalculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace bmiCalculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGeminiService _geminiService;

        public HomeController(ILogger<HomeController> logger, IGeminiService geminiService)
        {
            _logger = logger;
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult calculator()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Blogs()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShowData(IFormCollection f)
        {
            try
            {
                // Get unit selections
                string heightUnit = f["heightUnit"];
                string weightUnit = f["weightUnit"];

                // Parse age
                if (!int.TryParse(f["age"], out int age))
                {
                    ViewBag.Error = "Invalid age. Please enter a valid number.";
                    return View();
                }

                // Parse height based on unit
                double heightInCm = 0;
                double? feet = null, inches = null;

                if (heightUnit == "ft")
                {
                    // Handle feet and inches
                    if (!double.TryParse(f["feet"], out double feetValue) ||
                        !double.TryParse(f["inches"], out double inchesValue))
                    {
                        ViewBag.Error = "Invalid height. Please enter valid feet and inches.";
                        return View();
                    }

                    if (feetValue < 0 || inchesValue < 0 || inchesValue >= 12)
                    {
                        ViewBag.Error = "Invalid height. Inches should be between 0-11.";
                        return View();
                    }

                    feet = feetValue;
                    inches = inchesValue;

                    // Convert feet and inches to cm
                    double totalInches = (feetValue * 12) + inchesValue;
                    heightInCm = totalInches * 2.54; // Convert inches to cm

                    ViewBag.feet = feet;
                    ViewBag.inches = inches;
                }
                else
                {
                    // Handle centimeters
                    if (!double.TryParse(f["height"], out heightInCm))
                    {
                        ViewBag.Error = "Invalid height. Please enter a valid number.";
                        return View();
                    }
                }

                // Parse weight based on unit
                double weightInKg = 0;
                if (!double.TryParse(f["weight"], out double weight))
                {
                    ViewBag.Error = "Invalid weight. Please enter a valid number.";
                    return View();
                }

                if (weightUnit == "lbs")
                {
                    // Convert pounds to kg
                    weightInKg = weight * 0.453592;
                }
                else
                {
                    weightInKg = weight;
                }

                // Validate converted values
                if (heightInCm <= 0 || weightInKg <= 0 || age <= 0)
                {
                    ViewBag.Error = "All values must be greater than zero.";
                    return View();
                }

                // Store original values and units for display
                ViewBag.height = heightInCm;
                ViewBag.weight = weight;
                ViewBag.weightInKg = Math.Round(weightInKg, 2);
                ViewBag.age = age;
                ViewBag.heightUnit = heightUnit;
                ViewBag.weightUnit = weightUnit;

                // Calculate BMI
                double heightInMeters = heightInCm / 100.0; // Convert cm to meters
                double bmi = weightInKg / (heightInMeters * heightInMeters);
                ViewBag.BMI = Math.Round(bmi, 1); // Round to 1 decimal place

                // Determine BMI category and styling
                string category;
                string categoryClass;
                string description;

                if (bmi < 18.5)
                {
                    category = "Underweight";
                    categoryClass = "category-underweight";
                    description = "You may need to gain weight. Consider consulting with a healthcare provider for personalized advice.";
                }
                else if (bmi >= 18.5 && bmi < 25)
                {
                    category = "Normal Weight";
                    categoryClass = "category-normal";
                    description = "Great! You have a healthy weight. Maintain your current lifestyle with balanced diet and regular exercise.";
                }
                else if (bmi >= 25 && bmi < 30)
                {
                    category = "Overweight";
                    categoryClass = "category-overweight";
                    description = "You may benefit from losing some weight. Consider a balanced diet and increased physical activity.";
                }
                else
                {
                    category = "Obese";
                    categoryClass = "category-obese";
                    description = "It's recommended to consult with a healthcare provider for a personalized weight management plan.";
                }

                ViewBag.Category = category;
                ViewBag.CategoryClass = categoryClass;
                ViewBag.Description = description;

                // Create BMI data object for Gemini
                var bmiData = new BmiData
                {
                    BMI = Math.Round(bmi, 1),
                    HeightInCm = heightInCm,
                    WeightInKg = Math.Round(weightInKg, 2),
                    Age = age,
                    Category = category,
                    HeightUnit = heightUnit,
                    WeightUnit = weightUnit,
                    OriginalWeight = weight,
                    Feet = feet,
                    Inches = inches
                };

                // Get Gemini advice
                var geminiAdvice = await _geminiService.GetBmiAdviceAsync(bmiData);
                ViewBag.GeminiAdvice = geminiAdvice;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while processing your request. Please try again.";
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}