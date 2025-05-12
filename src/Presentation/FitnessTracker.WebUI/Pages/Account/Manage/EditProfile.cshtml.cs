using FitnessTracker.Application.DTOs.User; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FitnessTracker.WebUI.Pages.Account.Manage
{
    [Authorize]
    public class EditProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public UpdateUserProfileDto ProfileInput { get; set; } = new UpdateUserProfileDto();

        [BindProperty]
        public IFormFile? ProfilePictureFile { get; set; }

        public UserProfileDto? CurrentReadOnlyProfileData { get; set; } 

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
                ModelState.AddModelError(string.Empty, StatusMessage);
                return Page();
            }
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await apiClient.GetAsync("api/profiles/me");
                if (response.IsSuccessStatusCode)
                {
                    var fetchedProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (fetchedProfile != null)
                    {
                        CurrentReadOnlyProfileData = fetchedProfile;

                        ProfileInput.FirstName = fetchedProfile.FirstName;
                        ProfileInput.LastName = fetchedProfile.LastName;
                        ProfileInput.Bio = fetchedProfile.Bio;
                        ProfileInput.Location = fetchedProfile.Location;
                    }
                    else
                    {
                         StatusMessage = "Could not load your current profile data to edit.";
                         ModelState.AddModelError(string.Empty, StatusMessage);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string apiErrorMessage = $"Error loading profile for editing ({response.StatusCode}).";
                    try { var errorDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); if (!string.IsNullOrEmpty(errorDto?.Message)) apiErrorMessage = errorDto.Message; } catch {}
                    StatusMessage = apiErrorMessage;
                    ModelState.AddModelError(string.Empty, StatusMessage);
                }
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Error: Could not connect to the server to load your profile for editing.";
                ModelState.AddModelError(string.Empty, StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"An unexpected error occurred while loading profile for editing: {ex.Message}";
                ModelState.AddModelError(string.Empty, StatusMessage);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await EnsureReadOnlyDataIsLoadedAsync();
                return Page();
            }

            var apiClient = _httpClientFactory.CreateClient("ApiClient");
            var token = Request.Cookies["api_auth_token"];

            if (string.IsNullOrEmpty(token))
            {
                StatusMessage = "Error: Authentication token missing. Cannot update profile.";
                ModelState.AddModelError(string.Empty, StatusMessage);
                await EnsureReadOnlyDataIsLoadedAsync();
                return Page();
            }
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var formData = new MultipartFormDataContent();

            if (ProfileInput.FirstName != null) formData.Add(new StringContent(ProfileInput.FirstName), nameof(UpdateUserProfileDto.FirstName));
            if (ProfileInput.LastName != null) formData.Add(new StringContent(ProfileInput.LastName), nameof(UpdateUserProfileDto.LastName));
            if (ProfileInput.Bio != null) formData.Add(new StringContent(ProfileInput.Bio), nameof(UpdateUserProfileDto.Bio));
            if (ProfileInput.Location != null) formData.Add(new StringContent(ProfileInput.Location), nameof(UpdateUserProfileDto.Location));

            if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
            {
                var fileStreamContent = new StreamContent(ProfilePictureFile.OpenReadStream());
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(ProfilePictureFile.ContentType);
                formData.Add(fileStreamContent, "profilePictureFile", ProfilePictureFile.FileName); 
            }

            try
            {
                HttpResponseMessage response = await apiClient.PutAsync("api/profiles/me", formData);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["StatusMessage"] = "Your profile has been updated successfully.";
                    return RedirectToPage("./MyProfile"); 
                }
                else
                {
                    string apiErrorMessage = "Failed to update profile.";
                     if (!string.IsNullOrWhiteSpace(responseContent)) {
                        try {
                            var errorDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (!string.IsNullOrEmpty(errorDto?.Message)) apiErrorMessage = errorDto.Message;
                        } catch (JsonException) { }
                    }
                    StatusMessage = apiErrorMessage;
                    ModelState.AddModelError(string.Empty, apiErrorMessage);
                    await EnsureReadOnlyDataIsLoadedAsync();
                    return Page();
                }
            }
            catch (HttpRequestException) { StatusMessage = "HTTP Error updating profile."; ModelState.AddModelError(string.Empty, StatusMessage); await EnsureReadOnlyDataIsLoadedAsync(); return Page(); }
            catch (Exception ex) { StatusMessage = $"Unexpected error: {ex.Message}"; ModelState.AddModelError(string.Empty, StatusMessage); await EnsureReadOnlyDataIsLoadedAsync(); return Page(); }
        }

        private async Task EnsureReadOnlyDataIsLoadedAsync()
        {
            if (CurrentReadOnlyProfileData == null) 
            {
                var apiClient = _httpClientFactory.CreateClient("ApiClient");
                var token = Request.Cookies["api_auth_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    try
                    {
                        var response = await apiClient.GetAsync("api/profiles/me");
                        if (response.IsSuccessStatusCode)
                        {
                            CurrentReadOnlyProfileData = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                        }
                    }
                    catch {}
                }
            }
        }
    }
}