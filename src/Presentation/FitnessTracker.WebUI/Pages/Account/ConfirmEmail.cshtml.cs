using System.Text; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities; 
using FitnessTracker.Domain.Entities; 

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;
        public string? UserIdAttempted { get; set; }

        public async Task<IActionResult> OnGetAsync(string? userId, string? token)
        {
            UserIdAttempted = userId;

            if (userId == null || token == null)
            {
                StatusMessage = "Error: The email confirmation link is invalid or incomplete.";
                IsSuccess = false;
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = $"Error: We couldn't find an account associated with this confirmation link.";
                IsSuccess = false;
                return Page();
            }

            if (user.EmailConfirmed)
            {
                StatusMessage = "Your email has already been confirmed. You can log in.";
                IsSuccess = true; 
                return Page();
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException)
            {
                StatusMessage = "Error: The confirmation link appears to be corrupted. Please try registering again or request a new link.";
                IsSuccess = false;
                return Page();
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                StatusMessage = "Thank you for confirming your email!";
                IsSuccess = true;
            }
            else
            {
                var errorDescriptions = string.Join(", ", result.Errors.Select(e => e.Description));
                
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    StatusMessage = "Error: The confirmation link is invalid or has expired. You may need to request a new one.";
                }
                else if (result.Errors.Any(e => e.Code == "UserAlreadyHasPassword")) 
                {
                     StatusMessage = "Error processing request. Your email might already be confirmed.";
                }
                else
                {
                    StatusMessage = $"Error confirming your email. Please try again or contact support. ({errorDescriptions})";
                }
                IsSuccess = false;
            }

            return Page();
        }
    }
}