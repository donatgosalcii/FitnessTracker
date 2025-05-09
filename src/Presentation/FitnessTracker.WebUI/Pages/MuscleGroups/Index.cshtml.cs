using FitnessTracker.Application.DTOs.MuscleGroup; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace FitnessTracker.WebUI.Pages.MuscleGroups
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

        public IList<MuscleGroupDto> MuscleGroups { get; set; } = new List<MuscleGroupDto>();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Attempting to retrieve muscle groups for display.");

            string? apiToken = Request.Cookies["api_auth_token"];

            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API auth token cookie not found. Cannot fetch muscle groups.");
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync("api/musclegroups");

                if (response.IsSuccessStatusCode)
                {
                    var muscleGroupsList = await response.Content.ReadFromJsonAsync<List<MuscleGroupDto>>();
                    if (muscleGroupsList != null)
                    {
                        MuscleGroups = muscleGroupsList;
                        _logger.LogInformation("Successfully retrieved {Count} muscle groups.", MuscleGroups.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Muscle groups API call successful but response content was null or could not be deserialized.");
                        ErrorMessage = "Could not retrieve muscle groups at this time (empty response).";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Access to muscle groups API denied. Status: {StatusCode}. User might need to re-login or lacks permissions.", response.StatusCode);
                    ErrorMessage = "You are not authorized to view this content. Please ensure you are logged in with appropriate permissions.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to retrieve muscle groups. Status: {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
                    ErrorMessage = $"Error retrieving muscle groups: {response.ReasonPhrase} (Status: {response.StatusCode})";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching muscle groups.");
                ErrorMessage = "Could not connect to the service to retrieve muscle groups.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while fetching muscle groups.");
                ErrorMessage = "Error processing data from the muscle group service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching muscle groups.");
                ErrorMessage = "An unexpected error occurred.";
            }

            return Page();
        }
    }
}