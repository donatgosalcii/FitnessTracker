using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitnessTracker.Application.DTOs.Accounts;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(IHttpClientFactory httpClientFactory )
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public string? ReturnUrl { get; set; }

        public class InputModel { 
            [Required] [StringLength(50, MinimumLength = 1)] [Display(Name = "First Name")] public string FirstName { get; set; } = string.Empty;
            [Required] [StringLength(50, MinimumLength = 1)] [Display(Name = "Last Name")] public string LastName { get; set; } = string.Empty;
            [Required] [EmailAddress] [Display(Name = "Email")] public string Email { get; set; } = string.Empty;
            [Required] [StringLength(100, MinimumLength = 8)] [DataType(DataType.Password)] [Display(Name = "Password")] public string Password { get; set; } = string.Empty;
            [DataType(DataType.Password)] [Display(Name = "Confirm password")] [Compare("Password")] public string ConfirmPassword { get; set; } = string.Empty;
        }
        private class ApiRegisterSuccessResponse { 
            [JsonPropertyName("message")] public string? Message { get; set; }
        }
        private class ApiSimpleErrorResponse {
            [JsonPropertyName("message")] public string? Message { get; set; }
        }


        public void OnGet(string? returnUrl = null) { ReturnUrl = returnUrl; }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid) { return Page(); }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var registrationRequest = new UserRegistrationDto { /* ... populate ... */
                FirstName = Input.FirstName, LastName = Input.LastName, Email = Input.Email, Password = Input.Password, ConfirmPassword = Input.ConfirmPassword
            };

            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/register", registrationRequest);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("./RegistrationConfirmation", new { email = Input.Email });
                }
                else
                {
                    string errorMessage = $"Registration failed.";
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                        if (problemDetails?.Errors != null && problemDetails.Errors.Any())
                        {
                            foreach (var errorEntry in problemDetails.Errors) { foreach (var msg in errorEntry.Value) { ModelState.AddModelError(string.Empty, msg); } }
                            return Page();
                        }
                    }
                    try
                    {
                        var errorResponse = await response.Content.ReadFromJsonAsync<ApiSimpleErrorResponse>();
                        if (!string.IsNullOrEmpty(errorResponse?.Message)) { errorMessage = errorResponse.Message; }
                    }
                    catch (JsonException) 
                    {
                    }
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Could not connect to the registration service.");
                return Page();
            }
            catch (Exception) 
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                return Page();
            }
        }
    }
}