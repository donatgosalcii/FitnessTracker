using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.DTOs.MuscleGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessTracker.WebUI.Pages.Exercises;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty] public InputModel Input { get; set; } = new();

    public List<SelectListItem> AvailableMuscleGroups { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Exercise name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "At least one muscle group must be selected.")]
        [MinLength(1, ErrorMessage = "Please select at least one muscle group.")]
        public List<int> SelectedMuscleGroupIds { get; set; } = new();
    }

    private class ApiErrorResponse
    {
        public string? Message { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("Loading Create Exercise page for admin {User}.", User.Identity?.Name);
        await PopulateMuscleGroupsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Create Exercise POST failed model validation for admin {User}.", User.Identity?.Name);
            await PopulateMuscleGroupsAsync();
            return Page();
        }

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("Admin {User} attempting to create exercise but API token is missing.",
                User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Authentication token is missing. Please log in again.");
            await PopulateMuscleGroupsAsync();
            return Page();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var createExerciseDto = new CreateExerciseDto
        {
            Name = Input.Name,
            Description = Input.Description ?? string.Empty,
            MuscleGroupIds = Input.SelectedMuscleGroupIds
        };

        try
        {
            _logger.LogInformation("Admin {User} attempting to create exercise: {ExerciseName}", User.Identity?.Name,
                createExerciseDto.Name);
            var response = await apiClient.PostAsJsonAsync("api/exercises", createExerciseDto);

            if (response.IsSuccessStatusCode)
            {
                var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
                _logger.LogInformation(
                    "Exercise '{ExerciseName}' created successfully with ID {ExerciseId} by admin {User}.",
                    createdExercise?.Name, createdExercise?.Id, User.Identity?.Name);

                TempData["SuccessMessage"] = $"Exercise '{createdExercise?.Name}' created successfully.";
                return RedirectToPage("./Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "API call to create exercise failed for admin {User}. Status: {StatusCode}, Response: {Response}",
                User.Identity?.Name, response.StatusCode, errorContent);

            var apiErrorMessage = "An error occurred while creating the exercise.";
            try
            {
                var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(errorContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (validationProblemDetails?.Errors != null && validationProblemDetails.Errors.Any())
                {
                    foreach (var errorEntry in validationProblemDetails.Errors)
                    foreach (var msg in errorEntry.Value)
                        ModelState.AddModelError(string.Empty, msg);

                    apiErrorMessage = "Please correct the validation errors.";
                }
                else
                {
                    var simpleError = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrEmpty(simpleError?.Message))
                        apiErrorMessage = simpleError.Message;
                    else if (!string.IsNullOrEmpty(simpleError?.Title)) apiErrorMessage = simpleError.Title;
                }
            }
            catch (JsonException)
            {
            }

            ModelState.AddModelError(string.Empty, apiErrorMessage);
            await PopulateMuscleGroupsAsync();
            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while creating exercise: {ExerciseName} by admin {User}",
                createExerciseDto.Name, User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
            await PopulateMuscleGroupsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating exercise: {ExerciseName} by admin {User}",
                createExerciseDto.Name, User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            await PopulateMuscleGroupsAsync();
            return Page();
        }
    }

    private async Task PopulateMuscleGroupsAsync()
    {
        _logger.LogInformation("Populating available muscle groups for Create Exercise page.");
        var apiToken = Request.Cookies["api_auth_token"];

        if (string.IsNullOrEmpty(apiToken) && User.Identity != null && User.Identity.IsAuthenticated)
            _logger.LogWarning(
                "API token cookie not found while populating muscle groups, but user is authenticated via cookie. API call might fail if JWT is required.");


        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        if (!string.IsNullOrEmpty(apiToken))
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

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
                    _logger.LogInformation("Successfully populated {Count} muscle groups.",
                        AvailableMuscleGroups.Count);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Failed to populate muscle groups. API call to /api/musclegroups returned status {StatusCode}",
                    response.StatusCode);
                ModelState.AddModelError("MuscleGroupLoadError", "Could not load muscle groups for selection.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while populating muscle groups.");
            ModelState.AddModelError("MuscleGroupLoadError", "An error occurred while loading muscle groups.");
        }
    }
}