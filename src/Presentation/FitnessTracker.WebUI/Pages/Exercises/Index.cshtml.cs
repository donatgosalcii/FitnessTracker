using FitnessTracker.Application.DTOs.Exercise; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace FitnessTracker.WebUI.Pages.Exercises
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IList<ExerciseDto> Exercises { get; set; } = new List<ExerciseDto>();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Attempting to retrieve all exercises for display.");

            string? apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API auth token cookie not found. Cannot fetch exercises.");
                ErrorMessage = "Authentication token not found. Please log in.";
                return; 
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync("api/exercises");

                if (response.IsSuccessStatusCode)
                {
                    var exercisesList = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>();
                    if (exercisesList != null)
                    {
                        Exercises = exercisesList;
                        _logger.LogInformation("Successfully retrieved {Count} exercises.", Exercises.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Exercises API call successful but response content was null or could not be deserialized.");
                        ErrorMessage = "Could not retrieve exercises at this time (empty response).";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Access to exercises API denied. Status: {StatusCode}.", response.StatusCode);
                    ErrorMessage = "You are not authorized to view exercises. Please ensure you are logged in.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to retrieve exercises. Status: {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
                    ErrorMessage = $"Error retrieving exercises: {response.ReasonPhrase} (Status: {response.StatusCode})";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching exercises.");
                ErrorMessage = "Could not connect to the service to retrieve exercises.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching exercises.");
                ErrorMessage = "Error processing data from the exercise service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching exercises.");
                ErrorMessage = "An unexpected error occurred.";
            }
        }
    }
}