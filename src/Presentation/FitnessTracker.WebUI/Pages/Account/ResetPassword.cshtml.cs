using FitnessTracker.Application.DTOs.Account; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text; 
using System.Text.Json;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public ResetPasswordDto Input { get; set; } = new ResetPasswordDto();

        public string? StatusMessage { get; private set; }
        public bool IsSuccess { get; private set; }

        private class ApiResponseDto
        {
            public string? Message { get; set; }
        }

        public IActionResult OnGet(string? token = null, string? email = null)
        {
            if (token == null || email == null)
            {
                StatusMessage = "A code and email must be supplied for password reset.";
                IsSuccess = false;
                return Page(); 
            }
            else
            {
                Input.Token = token; 
                Input.Email = email;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));
            }
            catch (FormatException)
            {
                StatusMessage = "The password reset token is invalid or corrupted.";
                IsSuccess = false;
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }

            var apiRequestDto = new ResetPasswordDto
            {
                Email = Input.Email,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword,
                Token = decodedToken 
            };

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/reset-password", apiRequestDto);
                var responseContentString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Your password has been reset successfully. You can now log in.";
                    IsSuccess = true;
                    return Page(); 
                }
                else
                {
                    IsSuccess = false;
                    string errorMessage = "Failed to reset password. Please try again.";
                     if (!string.IsNullOrWhiteSpace(responseContentString)) {
                        try
                        {
                            var errorDto = JsonSerializer.Deserialize<ApiResponseDto>(responseContentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (!string.IsNullOrEmpty(errorDto?.Message))
                            {
                                errorMessage = errorDto.Message;
                            }
                        }
                        catch (JsonException) { }
                    }
                    StatusMessage = errorMessage;
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                IsSuccess = false;
                StatusMessage = "Could not connect to the server. Please try again later.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }
            catch (Exception)
            {
                IsSuccess = false;
                StatusMessage = "An unexpected error occurred. Please try again.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }
        }
    }
}