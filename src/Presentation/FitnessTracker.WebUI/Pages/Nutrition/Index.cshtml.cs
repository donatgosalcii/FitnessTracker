using FitnessTracker.Application.DTOs.Nutrition;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.WebUI.Pages.Nutrition
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        public DailyNutritionSummaryDto? DailySummary { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime SelectedDate { get; set; } = DateTime.Today; 

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: Unable to load user information.";
                return Page(); 
            }
            var userId = await _userManager.GetUserIdAsync(user);

            var result = await _nutritionService.GetDailyNutritionSummaryAsync(userId, SelectedDate);

            if (result.IsSuccess && result.Value != null)
            {
                DailySummary = result.Value;
            }
            else
            {
                DailySummary = new DailyNutritionSummaryDto(SelectedDate);
                if (result.IsFailure)
                {
                    StatusMessage = $"Error loading nutrition summary: {result.Error?.Message}";
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteLoggedItemAsync(int loggedItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid(); 

            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _nutritionService.RemoveLoggedFoodAsync(loggedItemId, userId);

            if (result.IsSuccess)
            {
                StatusMessage = "Logged item removed successfully.";
            }
            else
            {
                StatusMessage = $"Error removing item: {result.Error?.Message}";
            }
            return RedirectToPage(new { SelectedDate = this.SelectedDate.ToString("yyyy-MM-dd") });
        }
    }
}