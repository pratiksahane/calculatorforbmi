using System.Text;
using System.Text.Json;
using bmiCalculator.Models;

namespace bmiCalculator.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApi:ApiKey"];
            _logger = logger;
        }

        public async Task<string> GetBmiAdviceAsync(BmiData bmiData)
        {
            try
            {
                var prompt = CreatePrompt(bmiData);
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

                    return geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                           ?? "Unable to get advice at this time.";
                }
                else
                {
                    _logger.LogError($"Gemini API error: {response.StatusCode}");
                    return "Unable to get personalized advice at this time. Please consult with a healthcare provider.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Unable to get personalized advice at this time. Please consult with a healthcare provider.";
            }
        }

        private string CreatePrompt(BmiData bmiData)
        {
            return $@"As a health and fitness advisor, please provide personalized advice for someone with the following BMI information:

BMI: {bmiData.BMI}
Category: {bmiData.Category}
Age: {bmiData.Age}
Height: {bmiData.HeightInCm} cm
Weight: {bmiData.WeightInKg} kg

Please provide:
1. A brief assessment of their current BMI status
2. Specific dietary recommendations
3. Exercise suggestions appropriate for their BMI category
4. General health tips
5. Any precautions or recommendations to consult healthcare providers if needed

Keep the response concise, helpful, and encouraging. Limit to about 200-300 words.";
        }
    }
    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}