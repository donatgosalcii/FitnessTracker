using FitnessTracker.Application.DTOs.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FitnessTracker.WebUI.Pages.Account.Manage
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChangePasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public ChangePasswordDto Input { get; set; } = new ChangePasswordDto();

        [TempData]
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        private class ApiResponseDto { 
            public string? Message { get; set; }
            public string? Code { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            IsSuccess = false;
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");

            var token = Request.Cookies["api_auth_token"];

            if (string.IsNullOrEmpty(token))
            {
                StatusMessage = "Authentication token is missing. Please log in again.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page(); 
            }
            
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                HttpResponseMessage response = await apiClient.PostAsJsonAsync("api/account/change-password", Input);
                string responseContentString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Your password has been changed successfully.";
                    IsSuccess = true;
                    return RedirectToPage();
                }
                else
                {
                    string apiErrorMessage = "Failed to change password. Please check your input and try again.";
                    if (!string.IsNullOrWhiteSpace(responseContentString)) {
                        try
                        {
                            var errorDto = JsonSerializer.Deserialize<ApiResponseDto>(responseContentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (!string.IsNullOrEmpty(errorDto?.Message)) { apiErrorMessage = errorDto.Message; }
                        }
                        catch (JsonException) { }
                    }
                    StatusMessage = apiErrorMessage;
                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Could not connect to the server to change your password. Please try again later.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while trying to change your password. Please try again.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }
        }
    }
}