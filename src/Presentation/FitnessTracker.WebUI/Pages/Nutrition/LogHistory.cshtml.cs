using FitnessTracker.Application.Common;
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
    public class LogHistoryModel : PageModel
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogHistoryModel(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        public PagedResult<LoggedFoodItemDto>? LoggedItemsHistory { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        private const int DefaultPageSize = 10; 
        public int PageSize { get; set; } = DefaultPageSize;


        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? MealFilter { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }


        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: User not found."; 
                LoggedItemsHistory = new PagedResult<LoggedFoodItemDto> { PageNumber = CurrentPage, PageSize = PageSize }; 
                return;
            }
            var userId = await _userManager.GetUserIdAsync(user);

            if (CurrentPage < 1) CurrentPage = 1;

            var result = await _nutritionService.GetUserLoggedFoodHistoryAsync(userId, CurrentPage, PageSize, StartDate, EndDate, MealFilter);

            if (result.IsSuccess && result.Value != null)
            {
                LoggedItemsHistory = result.Value;
            }
            else
            {
                StatusMessage = result.IsFailure ? $"Error loading history: {result.Error?.Message}" : "No log history found matching your criteria.";
                LoggedItemsHistory = new PagedResult<LoggedFoodItemDto> { PageNumber = CurrentPage, PageSize = PageSize, TotalCount = 0, Items = new System.Collections.Generic.List<LoggedFoodItemDto>()}; 
            }
        }
            
        public async Task<IActionResult> OnPostDeleteLoggedItemAsync(int loggedItemId) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                StatusMessage = "Error: User not authorized.";
                return RedirectToPage(new { 
                    CurrentPage = this.CurrentPage, 
                    StartDate = this.StartDate?.ToString("yyyy-MM-dd"), 
                    EndDate = this.EndDate?.ToString("yyyy-MM-dd"), 
                    MealFilter = this.MealFilter 
                });
            }
            var userId = await _userManager.GetUserIdAsync(user);

            var result = await _nutritionService.RemoveLoggedFoodAsync(loggedItemId, userId);
            StatusMessage = result.IsSuccess ? "Logged item removed successfully." : $"Error removing item: {result.Error?.Message}";

            return RedirectToPage(new { 
                CurrentPage = this.CurrentPage, 
                StartDate = this.StartDate?.ToString("yyyy-MM-dd"), 
                EndDate = this.EndDate?.ToString("yyyy-MM-dd"), 
                MealFilter = this.MealFilter 
            });
        }
    }
}