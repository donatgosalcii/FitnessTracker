using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.Exercise; 
using FitnessTracker.Application.DTOs.MuscleGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace FitnessTracker.WebUI.Pages.Exercises
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

        public List<SelectListItem> AvailableMuscleGroups { get; set; } = new List<SelectListItem>();

        public class InputModel
        {
            [Required(ErrorMessage = "Exercise name is required.")]
            [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters.")]
            public string Name { get; set; } = string.Empty;

            [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "At least one muscle group must be selected.")]
            [MinLength(1, ErrorMessage = "Please select at least one muscle group.")]
            public List<int> SelectedMuscleGroupIds { get; set; } = new List<int>();
        }

        private class ApiErrorResponse
        {
            public string? Message { get; set; }
            public string? Title { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Loading exercise ID {ExerciseId} for editing by admin {User}.", Id, User.Identity?.Name);

            if (!await PopulateMuscleGroupsAsync())
            {
                TempData["ErrorMessagePage"] = "Could not load muscle groups. Editing an exercise is not possible without them.";
                return RedirectToPage("./Index");
            }

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for admin {User} loading exercise for edit.", User.Identity?.Name);
                return Challenge();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            try
            {
                var response = await apiClient.GetAsync($"api/exercises/{Id}");
                if (response.IsSuccessStatusCode)
                {
                    var exerciseDto = await response.Content.ReadFromJsonAsync<ExerciseDto>();
                    if (exerciseDto != null)
                    {
                        Input.Name = exerciseDto.Name;
                        Input.Description = exerciseDto.Description;
                        Input.SelectedMuscleGroupIds = exerciseDto.MuscleGroups?.Select(mg => mg.Id).ToList() ?? new List<int>();
                        return Page();
                    }
                    else
                    {
                        _logger.LogWarning("Exercise ID {ExerciseId} found by API but content was null.", Id);
                        TempData["ErrorMessagePage"] = "Could not load exercise details.";
                        return RedirectToPage("./Index");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Exercise ID {ExerciseId} not found by API.", Id);
                    TempData["ErrorMessagePage"] = $"Exercise with ID {Id} not found.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error fetching exercise ID {ExerciseId}. Status: {StatusCode}. Response: {ErrorContent}", Id, response.StatusCode, errorContent);
                    TempData["ErrorMessagePage"] = "Error loading exercise details.";
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching exercise ID {ExerciseId} for edit.", Id);
                TempData["ErrorMessagePage"] = "An unexpected error occurred while loading the exercise for editing.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update Exercise POST failed model validation for admin {User}.", User.Identity?.Name);
                await PopulateMuscleGroupsAsync(); 
                return Page();
            }

             _logger.LogInformation("Admin {User} attempting to update exercise ID {ExerciseId}", User.Identity?.Name, Id);

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API token missing for admin {User} updating exercise.", User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Authentication token is missing. Please log in again.");
                await PopulateMuscleGroupsAsync();
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            var updateExerciseDto = new UpdateExerciseDto
            {
                Name = Input.Name,
                Description = Input.Description ?? string.Empty,
                MuscleGroupIds = Input.SelectedMuscleGroupIds
            };

            try
            {
                HttpResponseMessage response = await apiClient.PutAsJsonAsync($"api/exercises/{Id}", updateExerciseDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Exercise ID {ExerciseId} updated successfully by admin {User}.", Id, User.Identity?.Name);
                    TempData["SuccessMessage"] = $"Exercise '{Input.Name}' (ID: {Id}) updated successfully.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API call to update exercise ID {ExerciseId} failed for admin {User}. Status: {StatusCode}, Response: {Response}", Id, User.Identity?.Name, response.StatusCode, errorContent);
                    
                    string apiErrorMessage = "An error occurred while updating the exercise.";
                    try
                    {
                        var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (validationProblemDetails?.Errors != null && validationProblemDetails.Errors.Any())
                        {
                            foreach (var errorEntry in validationProblemDetails.Errors)
                            {
                                foreach (var msg in errorEntry.Value) { ModelState.AddModelError(string.Empty, msg); }
                            }
                             apiErrorMessage = "Please correct the validation errors.";
                        } else {
                             var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                             if (!string.IsNullOrEmpty(simpleError?.Message)) { apiErrorMessage = simpleError.Message; }
                             else if (!string.IsNullOrEmpty(simpleError?.Title)) { apiErrorMessage = simpleError.Title; }
                        }
                    } catch(JsonException) { }

                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                    await PopulateMuscleGroupsAsync();
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while updating exercise ID {ExerciseId} by admin {User}", Id, User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
                await PopulateMuscleGroupsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating exercise ID {ExerciseId} by admin {User}", Id, User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                await PopulateMuscleGroupsAsync();
                return Page();
            }
        }

        private async Task<bool> PopulateMuscleGroupsAsync() 
        {
            _logger.LogInformation("Populating available muscle groups for Edit Exercise page.");
            var apiToken = Request.Cookies["api_auth_token"];
            var apiClient = _httpClientFactory.CreateClient("ApiClient");

            if (!string.IsNullOrEmpty(apiToken))
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            }
            else if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("API token cookie not found while populating muscle groups for Edit Exercise page, but user is authenticated via cookie.");
            }


            try
            {
                var response = await apiClient.GetAsync("api/musclegroups");
                if (response.IsSuccessStatusCode)
                {
                    var muscleGroups = await response.Content.ReadFromJsonAsync<List<MuscleGroupDto>>();
                    if (muscleGroups != null)
                    {
                        AvailableMuscleGroups = muscleGroups.Select(mg => new SelectListItem
                        {
                            Value = mg.Id.ToString(),
                            Text = mg.Name
                        }).ToList();
                        _logger.LogInformation("Successfully populated {Count} muscle groups for Edit Exercise page.", AvailableMuscleGroups.Count);
                        return true;
                    }
                     _logger.LogWarning("Muscle groups API call successful but content was null for Edit Exercise page.");
                }
                else
                {
                    _logger.LogWarning("Failed to populate muscle groups for Edit Exercise page. API status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while populating muscle groups for Edit Exercise page.");
            }
            ModelState.AddModelError("MuscleGroupLoadError", "Could not load muscle groups for selection. Please try again.");
            AvailableMuscleGroups = new List<SelectListItem>(); 
            return false;
        }
    }
}