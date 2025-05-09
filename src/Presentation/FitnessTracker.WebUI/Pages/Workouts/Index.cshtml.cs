using FitnessTracker.Application.DTOs.Workout; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace FitnessTracker.WebUI.Pages.Workouts
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

        public IList<WorkoutSummaryDto> Workouts { get; set; } = new List<WorkoutSummaryDto>();
        public string? PageErrorMessage { get; set; } 

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("User {User} attempting to retrieve their workout history.", User.Identity?.Name);

            string? apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API auth token cookie not found for user {User}. Cannot fetch workout history.", User.Identity?.Name);
                PageErrorMessage = "Authentication token not found. Please log in to view your workouts.";
                return Page(); 
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync("api/workouts"); 

                if (response.IsSuccessStatusCode)
                {
                    var workoutList = await response.Content.ReadFromJsonAsync<List<WorkoutSummaryDto>>();
                    if (workoutList != null)
                    {
                        Workouts = workoutList.OrderByDescending(w => w.DatePerformed).ToList(); 
                        _logger.LogInformation("Successfully retrieved {Count} workouts for user {User}.", Workouts.Count, User.Identity?.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Workout history API call successful for user {User} but response content was null or could not be deserialized.", User.Identity?.Name);
                        PageErrorMessage = "Could not retrieve workout history at this time (empty response).";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Access to workout history API denied for user {User}. Status: {StatusCode}.", User.Identity?.Name, response.StatusCode);
                    PageErrorMessage = "You are not authorized to view this content. Please ensure you are logged in.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to retrieve workout history for user {User}. Status: {StatusCode}. Response: {ErrorContent}", User.Identity?.Name, response.StatusCode, errorContent);
                    PageErrorMessage = $"Error retrieving workout history: {response.ReasonPhrase} (Status: {response.StatusCode})";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching workout history for user {User}.", User.Identity?.Name);
                PageErrorMessage = "Could not connect to the service to retrieve workout history.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching workout history for user {User}.", User.Identity?.Name);
                PageErrorMessage = "Error processing data from the workout service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching workout history for user {User}.", User.Identity?.Name);
                PageErrorMessage = "An unexpected error occurred.";
            }

            return Page();
        }
    }
}