using FitnessTracker.Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FitnessTracker.WebUI.Pages.Account.Manage
{
    [Authorize]
    public class MyProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MyProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public UserProfileDto? Profile { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        private class ApiErrorResponseDto { public string? Message { get; set; } }


        public async Task<IActionResult> OnGetAsync()
        {
            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var token = Request.Cookies["api_auth_token"];

            if (string.IsNullOrEmpty(token))
            {
                StatusMessage = "Error: Authentication token missing. Please log in again.";
                return Page(); 
            }
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await apiClient.GetAsync("api/profiles/me");
                if (response.IsSuccessStatusCode)
                {
                    Profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (Profile == null)
                    {
                        StatusMessage = "Could not load your profile data.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string apiErrorMessage = $"Error loading profile ({response.StatusCode}).";
                    try { var errorDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); if (!string.IsNullOrEmpty(errorDto?.Message)) apiErrorMessage = errorDto.Message; } catch {}
                    StatusMessage = apiErrorMessage;
                }
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Error: Could not connect to the server to load your profile.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"An unexpected error occurred: {ex.Message}";
            }
            return Page();
        }
    }
}