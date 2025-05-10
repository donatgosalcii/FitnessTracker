using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessTracker.WebUI.Pages.Account
{
    [AllowAnonymous]
    public class RegistrationConfirmationModel : PageModel
    {
        [BindProperty(SupportsGet = true)] 
        public string? Email { get; set; }

        public void OnGet(string? email = null)
        {
            if (!string.IsNullOrEmpty(email))
            {
                Email = email;
            }
        }
    }
}