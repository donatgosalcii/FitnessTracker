using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.MuscleGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.MuscleGroups
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "Muscle group name is required.")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
            public string Name { get; set; } = string.Empty;
        }

        private class ApiErrorResponse 
        {
            public string? Message { get; set; }
            public string? Title { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }


        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Loading muscle group ID {MuscleGroupId} for editing.", Id);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing while trying to load muscle group for edit.");
                return Challenge();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync($"api/musclegroups/{Id}");

                if (response.IsSuccessStatusCode)
                {
                    var muscleGroupDto = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
                    if (muscleGroupDto != null)
                    {
                        Input.Name = muscleGroupDto.Name;
                        return Page();
                    }
                    else
                    {
                        _logger.LogWarning("Muscle group ID {MuscleGroupId} found by API but content was null.", Id);
                        TempData["ErrorMessage"] = "Could not load muscle group details.";
                        return RedirectToPage("./Index");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Muscle group ID {MuscleGroupId} not found by API.", Id);
                    TempData["ErrorMessage"] = $"Muscle group with ID {Id} not found.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error while fetching muscle group ID {MuscleGroupId}. Status: {StatusCode}. Response: {ErrorContent}", Id, response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Error loading muscle group details.";
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching muscle group ID {MuscleGroupId} for edit.", Id);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading muscle group for editing.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            _logger.LogInformation("Attempting to update muscle group ID {MuscleGroupId} with new name {NewName}", Id, Input.Name);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                 _logger.LogWarning("API token missing while trying to update muscle group.");
                ModelState.AddModelError(string.Empty, "Authentication token is missing. Please log in again.");
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            var updateDto = new UpdateMuscleGroupDto { Name = Input.Name };

            try
            {
                HttpResponseMessage response = await apiClient.PutAsJsonAsync($"api/musclegroups/{Id}", updateDto);

                if (response.IsSuccessStatusCode)
                {
                     _logger.LogInformation("Muscle group ID {MuscleGroupId} updated successfully by admin {User}.", Id, User.Identity?.Name);
                    TempData["SuccessMessage"] = $"Muscle group '{Input.Name}' (ID: {Id}) updated successfully.";
                    return RedirectToPage("./Index");
                }
                else 
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API call to update muscle group ID {MuscleGroupId} failed. Status: {StatusCode}, Response: {Response}", Id, response.StatusCode, errorContent);
                    
                    string apiErrorMessage = "An error occurred while updating the muscle group.";
                    try
                    {
                        var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (validationProblemDetails?.Errors != null && validationProblemDetails.Errors.Any())
                        {
                            foreach (var errorEntry in validationProblemDetails.Errors)
                            {
                                foreach (var msg in errorEntry.Value)
                                {
                                    ModelState.AddModelError(string.Empty, msg);
                                }
                            }
                             apiErrorMessage = "Please correct the validation errors.";
                        }
                        else
                        {
                             var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                             if (!string.IsNullOrEmpty(simpleError?.Message)) { apiErrorMessage = simpleError.Message; }
                             else if (!string.IsNullOrEmpty(simpleError?.Title)) { apiErrorMessage = simpleError.Title; }
                        }
                    } catch(JsonException) { }

                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while updating muscle group ID {MuscleGroupId}", Id);
                ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating muscle group ID {MuscleGroupId}", Id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return Page();
            }
        }
    }
}