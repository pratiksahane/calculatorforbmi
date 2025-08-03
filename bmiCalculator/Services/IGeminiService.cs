using bmiCalculator.Models;

namespace bmiCalculator.Services
{
    public interface IGeminiService
    {
        Task<string> GetBmiAdviceAsync(BmiData bmiData);
    }
}