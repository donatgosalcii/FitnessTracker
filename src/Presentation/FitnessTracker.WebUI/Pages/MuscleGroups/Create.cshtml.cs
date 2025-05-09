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
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

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

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiToken = Request.Cookies["api_auth_token"];
            if (string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("Admin user attempting to create muscle group but API token is missing.");
                ModelState.AddModelError(string.Empty, "Authentication token is missing. Please log in again.");
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            var createDto = new CreateMuscleGroupDto { Name = Input.Name };

            try
            {
                _logger.LogInformation("Admin user {User} attempting to create muscle group: {Name}", User.Identity?.Name, createDto.Name);
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/musclegroups", createDto);

                if (response.IsSuccessStatusCode) 
                {
                    var createdMuscleGroup = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
                    _logger.LogInformation("Muscle group '{Name}' created successfully with ID {Id} by admin {User}.",
                        createdMuscleGroup?.Name, createdMuscleGroup?.Id, User.Identity?.Name);

                    TempData["SuccessMessage"] = $"Muscle group '{createdMuscleGroup?.Name}' created successfully.";
                    return RedirectToPage("./Index"); 
                }
                else 
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API call to create muscle group failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);

                    string apiErrorMessage = "An error occurred while creating the muscle group.";
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
                    }
                    catch (JsonException) {  }

                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while creating muscle group: {Name}", createDto.Name);
                ModelState.AddModelError(string.Empty, "Could not connect to the service. Please try again later.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating muscle group: {Name}", createDto.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return Page();
            }
        }
    }
}