using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using FitnessTracker.Domain.Entities; 

namespace FitnessTracker.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager; 

        public bool IsUserLoggedIn { get; set; }
        public string? UserFirstName { get; set; }

        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager) 
        {
            _logger = logger;
            _userManager = userManager; 
        }

        public async Task OnGetAsync() 
        {
            IsUserLoggedIn = User.Identity?.IsAuthenticated ?? false;

            if (IsUserLoggedIn)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    UserFirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email : user.FirstName;
                    _logger.LogInformation("Homepage loaded for authenticated user: {UserName}", UserFirstName);
                }
                else
                {
                    IsUserLoggedIn = false; 
                    _logger.LogWarning("User is authenticated but GetUserAsync returned null. User principal name: {PrincipalName}", User.Identity?.Name);
                }
            }
            else
            {
                 _logger.LogInformation("Homepage loaded for anonymous user.");
            }
        }
    }
}