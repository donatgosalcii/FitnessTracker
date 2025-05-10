using System.ComponentModel.DataAnnotations;
using System.Text.Json; 
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResendEmailConfirmationModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public string StatusMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public class InputModel {
             [Required] [EmailAddress] [Display(Name = "Your Email Address")] public string Email { get; set; } = string.Empty;
        }
        private class ApiResendResponse {
            [JsonPropertyName("message")] public string? Message { get; set; }
        }

        public void OnGet(string? email = null) { 
             if (!string.IsNullOrEmpty(email)) Input.Email = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var requestDto = new { Email = Input.Email };

            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/resend-confirmation", requestDto);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResendResponse>();
                StatusMessage = apiResponse?.Message ?? "An unexpected issue occurred.";
                IsSuccess = response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Could not connect to the service.";
                IsSuccess = false;
            }
            catch (JsonException)
            {
                StatusMessage = "There was an issue processing the server response.";
                IsSuccess = false;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred.";
                IsSuccess = false;
            }
            return Page();
        }
    }
}