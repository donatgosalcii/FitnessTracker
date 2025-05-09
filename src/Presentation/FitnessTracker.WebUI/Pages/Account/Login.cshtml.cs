using System.ComponentModel.DataAnnotations;
using System.Net.Http; // For HttpResponseMessage and IHttpClientFactory
using System.Net.Http.Json; // For PostAsJsonAsync, ReadFromJsonAsync
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks; // For Task
using FitnessTracker.Domain.Entities; // Assuming ApplicationUser is here
using Microsoft.AspNetCore.Authentication; // For HttpContext.SignInAsync if you go very custom
using Microsoft.AspNetCore.Identity;    // For SignInManager and UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // For ILogger
using Microsoft.AspNetCore.Http; // <-- ADDED for CookieOptions

namespace FitnessTracker.WebUI.Pages.Account
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(
            IHttpClientFactory httpClientFactory,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            // Optional:
            // [Display(Name = "Remember me?")]
            // public bool RememberMe { get; set; }
        }

        // This class should match the full structure of your API's LoginResponseDto
        private class ApiLoginSuccessResponse
        {
            [JsonPropertyName("isSuccess")]
            public bool IsSuccess { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("token")]
            public string? Token { get; set; }

            [JsonPropertyName("expiresOn")]
            public DateTimeOffset? ExpiresOn { get; set; } // Match your API DTO

            [JsonPropertyName("userId")]
            public string? UserId { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("firstName")]
            public string? FirstName { get; set; } // Match your API DTO

            [JsonPropertyName("lastName")]
            public string? LastName { get; set; } // Match your API DTO

            [JsonPropertyName("roles")]
            public List<string>? Roles { get; set; } // Match your API DTO
        }

        private class ApiErrorResponse
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var apiRequest = new { Input.Email, Input.Password };

            try
            {
                _logger.LogInformation("Attempting API login for user {Email}", Input.Email);
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/login", apiRequest);

                if (response.IsSuccessStatusCode)
                {
                    var loginApiResponse = await response.Content.ReadFromJsonAsync<ApiLoginSuccessResponse>();

                    if (loginApiResponse != null && loginApiResponse.IsSuccess && !string.IsNullOrEmpty(loginApiResponse.Token))
                    {
                        _logger.LogInformation("API login successful for {Email}. Token received. UserID: {UserId}", Input.Email, loginApiResponse.UserId);

                        ApplicationUser? user = null;
                        if (!string.IsNullOrEmpty(loginApiResponse.UserId))
                        {
                            user = await _userManager.FindByIdAsync(loginApiResponse.UserId);
                        }
                        else if (!string.IsNullOrEmpty(loginApiResponse.Email))
                        {
                            user = await _userManager.FindByEmailAsync(loginApiResponse.Email);
                        }

                        if (user != null)
                        {
                            // Local Identity sign-in (sets the .AspNetCore.Identity.Application cookie)
                            await _signInManager.SignInAsync(user, isPersistent: false /* Input.RememberMe */);
                            _logger.LogInformation("User {Email} (ID: {UserId}) signed in locally to cookie scheme.", user.Email, user.Id);

                            // --- Store the JWT in an HttpOnly cookie ---
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true, // Not accessible by client-side script
                                Secure = Request.IsHttps, // Only send over HTTPS if current request is HTTPS
                                SameSite = SameSiteMode.Strict, // Good for security
                                // Path = "/", // Default is usually fine
                            };

                            if (loginApiResponse.ExpiresOn.HasValue)
                            {
                                // Set cookie expiry to match JWT expiry if available
                                cookieOptions.Expires = loginApiResponse.ExpiresOn.Value;
                            }
                            // Name the cookie something specific
                            Response.Cookies.Append("api_auth_token", loginApiResponse.Token, cookieOptions);
                            _logger.LogInformation("JWT token stored in HttpOnly cookie 'api_auth_token' for user {Email}.", Input.Email);
                            // --- End JWT storage ---

                            return LocalRedirect(ReturnUrl);
                        }
                        else
                        {
                            _logger.LogError("API login successful for {Email}, but user (ID: {UserId}, Email: {ApiEmail}) not found locally for Identity sign-in.", Input.Email, loginApiResponse.UserId, loginApiResponse.Email);
                            ModelState.AddModelError(string.Empty, "Login successful, but there was an issue setting up your local session.");
                            return Page();
                        }
                    }
                    else
                    {
                        var failureMessage = loginApiResponse?.Message ?? "API login indicated failure or token was missing.";
                        _logger.LogWarning("API login response indicated failure for {Email}: {Reason}", Input.Email, failureMessage);
                        ModelState.AddModelError(string.Empty, failureMessage);
                        return Page();
                    }
                }
                else
                {
                    string errorMessage = $"Login failed.";
                    try
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                        if (!string.IsNullOrEmpty(errorResponse?.Message))
                        {
                            errorMessage = errorResponse.Message;
                        } else {
                             _logger.LogWarning("API error response for {Email} did not contain a 'message' field. Status: {StatusCode}", Input.Email, response.StatusCode);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Could not parse API error response body for {Email}. Status Code: {StatusCode}", Input.Email, response.StatusCode);
                        var rawError = await response.Content.ReadAsStringAsync();
                        _logger.LogDebug("Raw API error response: {RawError}", rawError);
                    }
                     _logger.LogWarning("API call failed for {Email}. Status: {StatusCode}, Message: {Message}", Input.Email, response.StatusCode, errorMessage);
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception during API login for {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Could not connect to the login service. Please try again later.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during API login process for {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
                return Page();
            }
        }
    }
}