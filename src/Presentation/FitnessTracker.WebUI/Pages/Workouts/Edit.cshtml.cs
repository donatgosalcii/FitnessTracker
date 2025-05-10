using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.DTOs.Workout;
using FitnessTracker.Application.DTOs.WorkoutSet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessTracker.WebUI.Pages.Workouts;

[Authorize]
public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    [BindProperty] public WorkoutInputModel Input { get; set; } = new();

    public List<SelectListItem> AvailableExercises { get; set; } = new();
    public string? PageErrorMessage { get; set; }

    public class WorkoutInputModel
    {
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Workout Date")]
        public DateTime DatePerformed { get; set; } = DateTime.Today;

        [StringLength(1000)]
        [Display(Name = "Overall Workout Notes")]
        public string? OverallNotes { get; set; }

        public List<SetInputModel> Sets { get; set; } = new();
    }

    public class SetInputModel
    {
        public Guid ClientId { get; set; } = Guid.NewGuid();
        public int? OriginalSetId { get; set; }

        [Required(ErrorMessage = "Please select an exercise.")]
        public int ExerciseId { get; set; }

        public string? ExerciseName { get; set; }

        [Required(ErrorMessage = "Set number is required.")]
        [Range(1, 100)]
        public int SetNumber { get; set; }

        [Range(0, 1000)] public int? Reps { get; set; }
        [Range(0, 10000)] public decimal? Weight { get; set; }

        [Display(Name = "Duration (s)")]
        [Range(0, 36000)]
        public int? DurationSeconds { get; set; }

        [Range(0, 1000)] public decimal? Distance { get; set; }

        [StringLength(500)]
        [Display(Name = "Set Notes")]
        public string? SetNotes { get; set; }

        public bool HasPerformanceMetric()
        {
            return Reps.HasValue || Weight.HasValue || DurationSeconds.HasValue || Distance.HasValue;
        }
    }

    private class ApiErrorResponse
    {
        public string? Message { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("User {User} loading workout ID {WorkoutId} for editing.", User.Identity?.Name, Id);

        if (!await PopulateExercisesAsync())
            PageErrorMessage = "Could not load available exercises. Some functionality may be limited.";

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for user {User} loading workout for edit.", User.Identity?.Name);
            PageErrorMessage = "Authentication token missing. Please log in again.";
            return Page();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        try
        {
            var response = await apiClient.GetAsync($"api/workouts/{Id}");
            if (response.IsSuccessStatusCode)
            {
                var workoutDto = await response.Content.ReadFromJsonAsync<WorkoutDetailDto>();
                if (workoutDto != null)
                {
                    Input.DatePerformed = workoutDto.DatePerformed;
                    Input.OverallNotes = workoutDto.Notes;
                    Input.Sets = workoutDto.Sets?.Select(s => new SetInputModel
                    {
                        ClientId = Guid.NewGuid(),
                        OriginalSetId = s.Id,
                        ExerciseId = s.ExerciseId,
                        ExerciseName = s.ExerciseName,
                        SetNumber = s.SetNumber,
                        Reps = s.Reps,
                        Weight = s.Weight,
                        DurationSeconds = s.DurationSeconds,
                        Distance = s.Distance,
                        SetNotes = s.Notes
                    }).ToList() ?? new List<SetInputModel>();
                    _logger.LogInformation("Successfully loaded workout ID {WorkoutId} for editing. User: {User}", Id,
                        User.Identity?.Name);
                }
                else
                {
                    _logger.LogWarning("Workout ID {WorkoutId} found by API but content was null. User: {User}", Id,
                        User.Identity?.Name);
                    PageErrorMessage = "Could not load workout details (empty response from server).";
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Workout ID {WorkoutId} not found or not accessible by user {User}.", Id,
                    User.Identity?.Name);
                PageErrorMessage = $"Workout with ID {Id} not found or access denied.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "API error fetching workout ID {WorkoutId}. Status: {StatusCode}. Response: {ErrorContent}. User: {User}",
                    Id, response.StatusCode, errorContent, User.Identity?.Name);
                PageErrorMessage = $"Error loading workout details (Status: {response.StatusCode}).";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching workout ID {WorkoutId} for edit. User: {User}", Id,
                User.Identity?.Name);
            PageErrorMessage = "An unexpected error occurred while loading the workout for editing.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("User {User} attempting to update workout ID {WorkoutId}.", User.Identity?.Name, Id);

        if (!await PopulateExercisesAsync())
        {
            PageErrorMessage = "Could not load available exercises. Please resolve this issue and try again.";
            return Page();
        }

        if (Input.Sets == null || !Input.Sets.Any())
            ModelState.AddModelError("Input.Sets", "A workout must contain at least one set.");
        else
            for (var i = 0; i < Input.Sets.Count; i++)
            {
                var set = Input.Sets[i];
                if (set.ExerciseId == 0)
                    ModelState.AddModelError($"Input.Sets[{i}].ExerciseId",
                        "An exercise must be selected for each set.");
                if (!set.HasPerformanceMetric())
                    ModelState.AddModelError($"Input.Sets[{i}]",
                        $"Set {set.SetNumber} for exercise '{set.ExerciseName ?? "selected"}' must have at least one performance metric (Reps, Weight, Duration, or Distance).");
            }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Update Workout POST failed model validation for user {User}, workout ID {WorkoutId}.",
                User.Identity?.Name, Id);
            return Page();
        }

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for user {User} trying to update workout ID {WorkoutId}.",
                User.Identity?.Name, Id);
            PageErrorMessage = "Authentication token missing. Please log in again.";
            return Page();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var updateWorkoutDto = new UpdateWorkoutDto
        {
            DatePerformed = Input.DatePerformed,
            Notes = Input.OverallNotes ?? string.Empty,
            Sets = Input.Sets!.Select(s => new LogWorkoutSetDto
            {
                ExerciseId = s.ExerciseId,
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                Weight = s.Weight,
                DurationSeconds = s.DurationSeconds,
                Distance = s.Distance,
                Notes = s.SetNotes ?? string.Empty
            }).ToList()
        };

        try
        {
            var response = await apiClient.PutAsJsonAsync($"api/workouts/{Id}", updateWorkoutDto);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Workout ID {WorkoutId} updated successfully by user {User}.", Id,
                    User.Identity?.Name);
                TempData["SuccessMessage"] = $"Workout (ID: {Id}) updated successfully!";
                return RedirectToPage("./Details", new { id = Id });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API call to update workout ID {WorkoutId} failed. User: {User}. Response: {Error}", Id,
                User.Identity?.Name, errorContent);

            var apiErrorMessage = "Failed to update workout.";
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

            ModelState.AddModelError(string.Empty, apiErrorMessage.Replace("exercise", "workout"));
            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception during workout update for ID {WorkoutId} by user {User}.", Id,
                User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception during workout update for ID {WorkoutId} by user {User}.", Id,
                User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving changes.");
            return Page();
        }
    }

    private async Task<bool> PopulateExercisesAsync()
    {
        _logger.LogInformation("Populating available exercises for workout page (User: {User}).", User.Identity?.Name);

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing, cannot populate exercises (User: {User}).", User.Identity?.Name);
            return false;
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        try
        {
            var response = await apiClient.GetAsync("api/exercises");
            if (response.IsSuccessStatusCode)
            {
                var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>();
                if (exercises != null)
                {
                    AvailableExercises = exercises.OrderBy(e => e.Name).Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Name
                    }).ToList();
                    _logger.LogInformation("Successfully populated {Count} exercises. User: {User}",
                        AvailableExercises.Count, User.Identity?.Name);
                    return true;
                }

                _logger.LogWarning(
                    "Exercises API call successful but content was null or failed to deserialize. User: {User}",
                    User.Identity?.Name);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to populate exercises. API call to /api/exercises returned status {StatusCode}. User: {User}",
                    response.StatusCode, User.Identity?.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception populating exercises. User: {User}", User.Identity?.Name);
        }

        AvailableExercises = new List<SelectListItem>();
        ModelState.AddModelError("ExerciseLoadError", "Could not load available exercises for selection.");
        return false;
    }
}