using FitnessTracker.Application.DTOs.Workout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace FitnessTracker.WebUI.Pages.Workouts
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public WorkoutDetailDto? Workout { get; private set; }
        public string? PageErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("User {User} attempting to retrieve details for workout ID {WorkoutId}.", User.Identity?.Name, Id);

            string? apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API auth token cookie not found for user {User} trying to view workout details.", User.Identity?.Name);
                PageErrorMessage = "Authentication token not found. Please log in.";
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync($"api/workouts/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    Workout = await response.Content.ReadFromJsonAsync<WorkoutDetailDto>();
                    if (Workout == null)
                    {
                        _logger.LogWarning("Workout details API call for ID {WorkoutId} by user {User} successful but response content was null or could not be deserialized.", Id, User.Identity?.Name);
                        PageErrorMessage = "Could not retrieve workout details at this time (empty response).";
                    }
                    else
                    {
                        _logger.LogInformation("Successfully retrieved details for workout ID {WorkoutId} for user {User}.", Id, User.Identity?.Name);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Workout ID {WorkoutId} not found or not accessible by user {User}.", Id, User.Identity?.Name);
                    PageErrorMessage = $"Workout with ID {Id} not found or you do not have permission to view it.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Access to workout details API for ID {WorkoutId} denied for user {User}. Status: {StatusCode}.", Id, User.Identity?.Name, response.StatusCode);
                    PageErrorMessage = "You are not authorized to view this workout. Please ensure you are logged in.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to retrieve workout details for ID {WorkoutId} for user {User}. Status: {StatusCode}. Response: {ErrorContent}", Id, User.Identity?.Name, response.StatusCode, errorContent);
                    PageErrorMessage = $"Error retrieving workout details: {response.ReasonPhrase} (Status: {response.StatusCode})";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching workout details for ID {WorkoutId} for user {User}.", Id, User.Identity?.Name);
                PageErrorMessage = "Could not connect to the service to retrieve workout details.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching workout details for ID {WorkoutId} for user {User}.", Id, User.Identity?.Name);
                PageErrorMessage = "Error processing data from the workout service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching workout details for ID {WorkoutId} for user {User}.", Id, User.Identity?.Name);
                PageErrorMessage = "An unexpected error occurred.";
            }

            if (Workout == null && string.IsNullOrEmpty(PageErrorMessage))
            {
                PageErrorMessage = $"Workout with ID {Id} could not be loaded.";
            }
            
            return Page();
        }
    }
}