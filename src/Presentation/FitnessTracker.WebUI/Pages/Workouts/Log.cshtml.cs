using System.ComponentModel.DataAnnotations;
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
public class LogModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LogModel> _logger;

    public LogModel(IHttpClientFactory httpClientFactory, ILogger<LogModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

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

        [Required(ErrorMessage = "Please select an exercise.")]
        public int ExerciseId { get; set; }

        public string? ExerciseName { get; set; }

        [Required(ErrorMessage = "Set number is required.")]
        [Range(1, 100, ErrorMessage = "Set number must be between 1 and 100.")]
        public int SetNumber { get; set; }

        [Range(0, 1000, ErrorMessage = "Reps must be between 0 and 1000.")]
        public int? Reps { get; set; }

        [Range(0, 10000, ErrorMessage = "Weight must be between 0 and 10000.")]
        public decimal? Weight { get; set; }

        [Range(0, 36000, ErrorMessage = "Duration must be between 0 and 36000 seconds.")]
        [Display(Name = "Duration (seconds)")]
        public int? DurationSeconds { get; set; }

        [Range(0, 1000, ErrorMessage = "Distance must be between 0 and 1000.")]
        public decimal? Distance { get; set; }

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
        _logger.LogInformation("Loading Log Workout page for user {User}.", User.Identity?.Name);
        Input.DatePerformed = DateTime.Today;

        if (!await PopulateExercisesAsync()) PageErrorMessage = "Could not load exercises. Please try again later.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Attempting to save workout for user {User}.", User.Identity?.Name);

        if (!await PopulateExercisesAsync())
        {
            PageErrorMessage = "Could not load exercises to validate form. Please try again later.";
            return Page();
        }

        if (Input.Sets == null || !Input.Sets.Any())
            ModelState.AddModelError("Input.Sets", "Please add at least one set to the workout.");
        else
            for (var i = 0; i < Input.Sets.Count; i++)
            {
                var set = Input.Sets[i];
                if (set.ExerciseId == 0)
                    ModelState.AddModelError($"Input.Sets[{i}].ExerciseId",
                        "An exercise must be selected for each set.");
                if (!set.HasPerformanceMetric())
                    ModelState.AddModelError($"Input.Sets[{i}]",
                        $"Set {set.SetNumber} for exercise '{set.ExerciseName}' must have at least one performance metric (Reps, Weight, Duration, or Distance).");
            }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Log Workout POST failed model validation for user {User}.", User.Identity?.Name);
            return Page();
        }

        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for user {User} trying to log workout.", User.Identity?.Name);
            PageErrorMessage = "Authentication token missing. Please log in again.";
            return Page();
        }

        var apiClient = _httpClientFactory.CreateClient("ApiClient");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var logWorkoutDto = new LogWorkoutDto
        {
            DatePerformed = Input.DatePerformed,
            Notes = Input.OverallNotes ?? string.Empty,
            Sets = Input.Sets!
                .Select(s => new LogWorkoutSetDto
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
            var response = await apiClient.PostAsJsonAsync("api/workouts", logWorkoutDto);

            if (response.IsSuccessStatusCode)
            {
                var createdWorkout = await response.Content.ReadFromJsonAsync<WorkoutDetailDto>();
                _logger.LogInformation("Workout logged successfully with ID {WorkoutId} for user {User}.",
                    createdWorkout?.Id, User.Identity?.Name);
                TempData["SuccessMessage"] =
                    $"Workout on {createdWorkout?.DatePerformed:yyyy-MM-dd} logged successfully!";
                return RedirectToPage("./Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "API call to log workout failed for user {User}. Status: {StatusCode}, Response: {Response}",
                User.Identity?.Name, response.StatusCode, errorContent);
            var apiErrorMessage = "An error occurred while logging the workout.";
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
                /* Ignore */
            }

            ModelState.AddModelError(string.Empty, apiErrorMessage);
            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while logging workout for user {User}", User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while logging workout for user {User}", User.Identity?.Name);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            return Page();
        }
    }

    private async Task<bool> PopulateExercisesAsync()
    {
        _logger.LogInformation("Populating available exercises for Log Workout page for user {User}.",
            User.Identity?.Name);
        var apiToken = Request.Cookies["api_auth_token"];
        if (string.IsNullOrEmpty(apiToken))
        {
            _logger.LogWarning("API token missing for user {User} trying to populate exercises.", User.Identity?.Name);
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
                    _logger.LogInformation("Successfully populated {Count} exercises.", AvailableExercises.Count);
                    return true;
                }

                _logger.LogWarning("Exercises API call successful but content was null.");
            }
            else
            {
                _logger.LogWarning(
                    "Failed to populate exercises. API call to /api/exercises returned status {StatusCode}",
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while populating exercises.");
        }

        AvailableExercises = new List<SelectListItem>();
        return false;
    }
}