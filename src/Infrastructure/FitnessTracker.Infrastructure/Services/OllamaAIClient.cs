using System.Text;
using System.Text.Json;
using FitnessTracker.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Infrastructure.Services
{
    public class OllamaAIClient : IAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private bool _isAvailable;
        
        public OllamaAIClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _isAvailable = CheckAvailability();
        }
        
        public async Task<string> GetResponseAsync(string message, string? systemPrompt = null)
        {
            try
            {
                string prompt = systemPrompt ?? _configuration["Ollama:SystemPrompt"] ?? "You are a fitness assistant.";
                string userPrompt = $"{prompt} User question: {message}";
                
                var requestData = new
                {
                    model = _configuration["Ollama:Model"] ?? "llama3",
                    prompt = userPrompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.7,
                        top_p = 0.9,
                        max_tokens = 500
                    }
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );
                
                string baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
                var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", content);
                response.EnsureSuccessStatusCode();
                
                var responseBody = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseBody);
                
                if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    return responseElement.GetString() ?? "I couldn't generate a fitness-related response. Please try again.";
                }
                
                return "Sorry, I couldn't understand the response from the AI service.";
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public bool IsAvailable()
        {
            return _isAvailable;
        }
        
        private bool CheckAvailability()
        {
            try
            {
                string baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
                var response = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, baseUrl)).Result;
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 