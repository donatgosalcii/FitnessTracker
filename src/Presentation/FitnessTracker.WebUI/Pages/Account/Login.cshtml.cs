using System.ComponentModel.DataAnnotations;
using System.Text.Json; 
using System.Text.Json.Serialization;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(
            IHttpClientFactory httpClientFactory,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _httpClientFactory = httpClientFactory;
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
        }

        private class ApiLoginSuccessResponse {
             [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; } [JsonPropertyName("message")] public string? Message { get; set; } [JsonPropertyName("token")] public string? Token { get; set; } [JsonPropertyName("expiresOn")] public DateTimeOffset? ExpiresOn { get; set; } [JsonPropertyName("userId")] public string? UserId { get; set; } [JsonPropertyName("email")] public string? Email { get; set; } [JsonPropertyName("firstName")] public string? FirstName { get; set; } [JsonPropertyName("lastName")] public string? LastName { get; set; } [JsonPropertyName("roles")] public List<string>? Roles { get; set; }
        }
        private class ApiErrorResponse {
             [JsonPropertyName("message")] public string? Message { get; set; }
        }

        public void OnGet(string? returnUrl = null) { 
            if (!string.IsNullOrEmpty(ErrorMessage)) { ModelState.AddModelError(string.Empty, ErrorMessage); } ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid) { return Page(); }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var apiRequest = new { Input.Email, Input.Password };

            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/login", apiRequest);

                if (response.IsSuccessStatusCode) 
                {
                    var loginApiResponse = await response.Content.ReadFromJsonAsync<ApiLoginSuccessResponse>();
                    if (loginApiResponse != null && loginApiResponse.IsSuccess && !string.IsNullOrEmpty(loginApiResponse.Token))
                    {
                        ApplicationUser? user = null;
                        if (!string.IsNullOrEmpty(loginApiResponse.UserId)) { user = await _userManager.FindByIdAsync(loginApiResponse.UserId); }
                        else if (!string.IsNullOrEmpty(loginApiResponse.Email)) { user = await _userManager.FindByEmailAsync(loginApiResponse.Email); }

                        if (user != null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            var cookieOptions = new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, };
                            if (loginApiResponse.ExpiresOn.HasValue) { cookieOptions.Expires = loginApiResponse.ExpiresOn.Value; }
                            Response.Cookies.Append("api_auth_token", loginApiResponse.Token, cookieOptions);
                            return LocalRedirect(ReturnUrl);
                        }
                        else { ModelState.AddModelError(string.Empty, "Login successful, but local session setup failed."); return Page(); }
                    }
                    else 
                    {
                        var failureMessage = loginApiResponse?.Message ?? "API login indicated failure.";
                        ModelState.AddModelError(string.Empty, failureMessage);
                        if (failureMessage.Contains("confirm your email", StringComparison.OrdinalIgnoreCase)) { ViewData["ShowResendConfirmationLink"] = true; ViewData["UserEmailForResend"] = Input.Email; }
                        return Page();
                    }
                }
                else 
                {
                    string errorMessage = $"Login failed.";
                    try
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                        if (!string.IsNullOrEmpty(errorResponse?.Message)) { errorMessage = errorResponse.Message; }
                    }
                    catch (JsonException) 
                    {
                    }
                    ModelState.AddModelError(string.Empty, errorMessage);
                    if (errorMessage.Contains("confirm your email", StringComparison.OrdinalIgnoreCase)) { ViewData["ShowResendConfirmationLink"] = true; ViewData["UserEmailForResend"] = Input.Email; }
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the login service.");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
                return Page();
            }
        }
    }
}