using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FitnessTracker.Domain.Entities; // Your user entity
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // <-- ADDED for Response.Cookies

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // It's good practice to handle GET to logout by just redirecting,
            // or by showing a confirmation page that then POSTs.
            // For simplicity, we can make GET also log out if that's acceptable.
            // However, POST is more standard for actions that change state.
            // Let's assume the form in _LoginPartial.cshtml POSTs.
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            // 1. Sign out from the ASP.NET Core Identity cookie scheme
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out from Identity cookie scheme.");

            // 2. Delete the custom api_auth_token cookie
            if (Request.Cookies.ContainsKey("api_auth_token")) // Check if it exists
            {
                // To delete a cookie, you append it again with an expiry date in the past.
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1), // Set to past date
                    HttpOnly = true, // Match original settings
                    Secure = Request.IsHttps, // Match original settings
                    SameSite = SameSiteMode.Strict // Match original settings
                };
                Response.Cookies.Append("api_auth_token", "", cookieOptions); // Empty value, expired
                _logger.LogInformation("api_auth_token cookie cleared.");
            }
            else
            {
                _logger.LogInformation("api_auth_token cookie not found, no action taken for it.");
            }


            // 3. Redirect the user
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
    }
}