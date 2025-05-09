using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.MuscleGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.MuscleGroups
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public MuscleGroupDto? MuscleGroup { get; private set; }

        private class ApiErrorResponse
        {
            public string? Message { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Loading muscle group ID {MuscleGroupId} for delete confirmation.", Id);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for delete confirmation page.");
                return Challenge(); 
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync($"api/musclegroups/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    MuscleGroup = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
                    if (MuscleGroup == null)
                    {
                        _logger.LogWarning("Muscle group ID {MuscleGroupId} found by API but content was null (delete page).", Id);
                        return RedirectToPage("./Index"); 
                    }
                    return Page();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Muscle group ID {MuscleGroupId} not found by API (delete page).", Id);
                    return NotFound($"Muscle group with ID {Id} not found."); 
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error fetching muscle group ID {MuscleGroupId} for delete. Status: {StatusCode}. Response: {ErrorContent}", Id, response.StatusCode, errorContent);
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching muscle group ID {MuscleGroupId} for delete.", Id);
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync() 
        {
            _logger.LogInformation("Attempting to delete muscle group ID {MuscleGroupId} by admin {User}.", Id, User.Identity?.Name);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for delete action.");
                ModelState.AddModelError(string.Empty, "Authentication token missing.");
                await OnGetAsync();
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                HttpResponseMessage response = await apiClient.DeleteAsync($"api/musclegroups/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Muscle group ID {MuscleGroupId} deleted successfully by admin {User}.", Id, User.Identity?.Name);
                    return RedirectToPage("./Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("API call to delete muscle group ID {MuscleGroupId} returned NotFound.", Id);
                    ModelState.AddModelError(string.Empty, $"Muscle group with ID {Id} not found or already deleted.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API call to delete muscle group ID {MuscleGroupId} failed. Status: {StatusCode}. Response: {Response}", Id, response.StatusCode, errorContent);
                    
                    string apiErrorMessage = "Could not delete muscle group.";
                     try {
                        var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (!string.IsNullOrEmpty(simpleError?.Message)) { apiErrorMessage = simpleError.Message; }
                    } catch(JsonException) {  }

                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while deleting muscle group ID {MuscleGroupId}", Id);
                ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting muscle group ID {MuscleGroupId}", Id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }

            await OnGetAsync(); 
            return Page();
        }
    }
}