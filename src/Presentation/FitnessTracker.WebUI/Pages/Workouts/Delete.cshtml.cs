using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.Workout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.Workouts;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public WorkoutDetailDto? WorkoutToDelete { get; private set; }
    public string? PageErrorMessage { get; set; }


    private class ApiErrorResponse
    {
        public string? Message { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("User {User} loading workout ID {WorkoutId} for delete confirmation.",
            User.Identity?.Name, Id);

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for user {User} on delete workout confirmation page.",
                User.Identity?.Name);
            return Challenge();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        try
        {
            var response = await apiClient.GetAsync($"api/workouts/{Id}");

            if (response.IsSuccessStatusCode)
            {
                WorkoutToDelete = await response.Content.ReadFromJsonAsync<WorkoutDetailDto>();
                if (WorkoutToDelete == null)
                {
                    _logger.LogWarning(
                        "Workout ID {WorkoutId} found by API for user {User} but content was null (delete page).", Id,
                        User.Identity?.Name);
                    TempData["ErrorMessagePage"] = "Could not load workout details for deletion.";
                    return RedirectToPage("./Index");
                }

                return Page();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Workout ID {WorkoutId} not found or not accessible by user {User} (delete page).",
                    Id, User.Identity?.Name);
                TempData["ErrorMessagePage"] = $"Workout with ID {Id} not found or you do not have permission.";
                return RedirectToPage("./Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "API error fetching workout ID {WorkoutId} for delete by user {User}. Status: {StatusCode}. Response: {ErrorContent}",
                Id, User.Identity?.Name, response.StatusCode, errorContent);
            TempData["ErrorMessagePage"] = "Error loading workout for deletion.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching workout ID {WorkoutId} for delete by user {User}.", Id,
                User.Identity?.Name);
            TempData["ErrorMessagePage"] = "An unexpected error occurred while preparing for workout deletion.";
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("User {User} attempting to delete workout ID {WorkoutId}.", User.Identity?.Name, Id);

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for delete workout action by user {User}.", User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Authentication token missing.");
            await PreparePageModelForRedisplayAsync();
            return Page();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        try
        {
            var response = await apiClient.DeleteAsync($"api/workouts/{Id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Workout ID {WorkoutId} deleted successfully by user {User}.", Id,
                    User.Identity?.Name);
                TempData["SuccessMessage"] = $"Workout with ID {Id} deleted successfully.";
                return RedirectToPage("./Index");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("API call to delete workout ID {WorkoutId} returned NotFound for user {User}.", Id,
                    User.Identity?.Name);
                ModelState.AddModelError(string.Empty,
                    $"Workout with ID {Id} not found or already deleted, or you do not have permission.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "API call to delete workout ID {WorkoutId} failed for user {User}. Status: {StatusCode}. Response: {Response}",
                    Id, User.Identity?.Name, response.StatusCode, errorContent);

                var apiErrorMessage = "Could not delete workout.";
                try
                {
                    var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrEmpty(simpleError?.Message)) apiErrorMessage = simpleError.Message;
                }
                catch (JsonException)
                {
                    /* Ignore */
                }

                ModelState.AddModelError(string.Empty, apiErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while deleting workout ID {WorkoutId} by user {User}", Id,
                User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting workout ID {WorkoutId} by user {User}", Id,
                User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
        }

        await PreparePageModelForRedisplayAsync();
        return Page();
    }

    private async Task PreparePageModelForRedisplayAsync()
    {
        if (WorkoutToDelete == null && Id > 0)
        {
            _logger.LogInformation("Re-populating workout details for ID {WorkoutId} after POST failure.", Id);
            var apiToken = Request.Cookies["api_auth_token"];
            if (!string.IsNullOrEmpty(apiToken))
            {
                var apiClient = _httpClientFactory.CreateClient("ApiClient");
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                var response = await apiClient.GetAsync($"api/workouts/{Id}");
                if (response.IsSuccessStatusCode)
                    WorkoutToDelete = await response.Content.ReadFromJsonAsync<WorkoutDetailDto>();
                else
                    _logger.LogWarning(
                        "Failed to re-populate workout details for ID {WorkoutId} after POST failure. API Status: {StatusCode}",
                        Id, response.StatusCode);
            }
            else
            {
                _logger.LogWarning(
                    "API token missing, cannot re-populate workout details for ID {WorkoutId} after POST failure.", Id);
            }
        }
    }
}