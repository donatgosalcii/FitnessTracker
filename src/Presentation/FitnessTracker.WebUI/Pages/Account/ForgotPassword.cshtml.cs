using FitnessTracker.Application.DTOs.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory )
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public ForgotPasswordDto Input { get; set; } = new ForgotPasswordDto();

        public string? StatusMessage { get; private set; }

        private class ApiResponseDto
        {
            public string? Message { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            string privacyPreservingMessage = "If an account with that email address exists, a password reset link has been sent. Please check your inbox (and spam folder).";

            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/forgot-password", Input);
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        StatusMessage = apiResponse?.Message ?? privacyPreservingMessage;
                    }
                    catch (JsonException) 
                    {
                        StatusMessage = privacyPreservingMessage;
                    }
                }
                else 
                {
                    StatusMessage = privacyPreservingMessage;
                }
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Could not connect to the server. Please try again later.";
            }
            catch (JsonException) 
            {
                StatusMessage = privacyPreservingMessage; 
            }
            catch (Exception) 
            {
                StatusMessage = "An unexpected error occurred processing your request. Please try again.";
            }
            return Page();
        }
    }
}