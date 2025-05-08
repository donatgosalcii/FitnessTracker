using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization; 
using FitnessTracker.Application.DTOs.Accounts;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous] 
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            IHttpClientFactory httpClientFactory,
            ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

         private class ApiErrorResponse
        {
            public string? Title { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; } 

             [System.Text.Json.Serialization.JsonPropertyName("message")]
             public string? Message { get; set; }

        }


        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");

            var registrationRequest = new UserRegistrationDto
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword 
            };

            try
            {
                _logger.LogInformation("Attempting API registration for user {Email}", Input.Email);
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/register", registrationRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API registration successful for {Email}.", Input.Email);

                    TempData["StatusMessage"] = "Registration successful! Please log in.";
                    return RedirectToPage("./Login"); 
                }
                else 
                {
                    string errorMessage = $"Registration failed. Status: {response.StatusCode}";
                    try
                    {
                        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

                        if (problemDetails?.Errors != null && problemDetails.Errors.Any())
                        {
                            foreach (var errorEntry in problemDetails.Errors)
                            {
                                foreach (var msg in errorEntry.Value)
                                {
                                    ModelState.AddModelError(string.Empty, msg);
                                    _logger.LogWarning("API Registration Error for {Email}: {Field} - {Error}", Input.Email, errorEntry.Key, msg);
                                }
                            }
                            errorMessage = "Registration failed. Please check the errors below.";
                        } else {
                            var simpleError = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                            if (!string.IsNullOrEmpty(simpleError?.Message)) {
                                errorMessage = simpleError.Message;
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Could not parse API registration error response body. Status Code: {StatusCode}", response.StatusCode);
                        var rawError = await response.Content.ReadAsStringAsync();
                        _logger.LogDebug("Raw API error response: {RawError}", rawError);
                        errorMessage = $"Registration failed with status {response.StatusCode}."; 
                    }

                    _logger.LogWarning("API registration failed for {Email}. Reason: {Reason}", Input.Email, errorMessage);
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error during API registration for {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Could not connect to the registration service.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during API registration process for {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                return Page();
            }
        }
    }
}