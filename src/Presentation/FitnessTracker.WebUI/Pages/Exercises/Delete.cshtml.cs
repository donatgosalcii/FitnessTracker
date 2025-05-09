using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.Exercise; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.Exercises
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

        public ExerciseDto? Exercise { get; private set; }

        private class ApiErrorResponse
        {
            public string? Message { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Admin {User} loading exercise ID {ExerciseId} for delete confirmation.", User.Identity?.Name, Id);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for delete exercise confirmation page by admin {User}.", User.Identity?.Name);
                return Challenge();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync($"api/exercises/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    Exercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
                    if (Exercise == null)
                    {
                        _logger.LogWarning("Exercise ID {ExerciseId} found by API but content was null (delete page).", Id);
                        TempData["ErrorMessagePage"] = "Could not load exercise details for deletion.";
                        return RedirectToPage("./Index");
                    }
                    return Page();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Exercise ID {ExerciseId} not found by API (delete page).", Id);
                    TempData["ErrorMessagePage"] = $"Exercise with ID {Id} not found.";
                    return NotFound($"Exercise with ID {Id} not found.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error fetching exercise ID {ExerciseId} for delete by admin {User}. Status: {StatusCode}. Response: {ErrorContent}", Id, User.Identity?.Name, response.StatusCode, errorContent);
                    TempData["ErrorMessagePage"] = "Error loading exercise for deletion.";
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching exercise ID {ExerciseId} for delete by admin {User}.", Id, User.Identity?.Name);
                TempData["ErrorMessagePage"] = "An unexpected error occurred.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Admin {User} attempting to delete exercise ID {ExerciseId}.", User.Identity?.Name, Id);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for delete exercise action by admin {User}.", User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Authentication token missing.");
                await PreparePageModelForRedisplayAsync(); 
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                HttpResponseMessage response = await apiClient.DeleteAsync($"api/exercises/{Id}");

                if (response.IsSuccessStatusCode) 
                {
                    _logger.LogInformation("Exercise ID {ExerciseId} deleted successfully by admin {User}.", Id, User.Identity?.Name);
                    TempData["SuccessMessage"] = $"Exercise with ID {Id} deleted successfully.";
                    return RedirectToPage("./Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("API call to delete exercise ID {ExerciseId} returned NotFound for admin {User}.", Id, User.Identity?.Name);
                    ModelState.AddModelError(string.Empty, $"Exercise with ID {Id} not found or already deleted.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                     _logger.LogWarning("API call to delete exercise ID {ExerciseId} failed for admin {User}. Status: {StatusCode}. Response: {Response}", Id, User.Identity?.Name, response.StatusCode, errorContent);
                    
                    string apiErrorMessage = "Could not delete exercise.";
                     try {
                        var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (!string.IsNullOrEmpty(simpleError?.Message)) { apiErrorMessage = simpleError.Message; }
                    } catch(JsonException) { /* Ignore */ }
                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while deleting exercise ID {ExerciseId} by admin {User}", Id, User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting exercise ID {ExerciseId} by admin {User}", Id, User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }

            await PreparePageModelForRedisplayAsync();
            return Page();
        }

        private async Task PreparePageModelForRedisplayAsync()
        {
            if (Exercise == null && Id > 0) 
            {
                _logger.LogInformation("Re-populating exercise details for ID {ExerciseId} after POST failure.", Id);
                var apiToken = Request.Cookies["api_auth_token"];
                if (!string.IsNullOrEmpty(apiToken))
                {
                    var apiClient = _httpClientFactory.CreateClient("ApiClient");
                    apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                    var response = await apiClient.GetAsync($"api/exercises/{Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        Exercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
                    }
                    else
                    {
                         _logger.LogWarning("Failed to re-populate exercise details for ID {ExerciseId} after POST failure. API Status: {StatusCode}", Id, response.StatusCode);
                    }
                } else {
                    _logger.LogWarning("API token missing, cannot re-populate exercise details for ID {ExerciseId} after POST failure.", Id);
                }
            }
        }
    }
}