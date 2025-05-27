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
    public class LogFoodModel : PageModel
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogFoodModel(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IList<FoodItemDto> SearchResults { get; set; } = new List<FoodItemDto>();

        [BindProperty]
        public LogFoodInputModel Input { get; set; } = new LogFoodInputModel();
        
        [TempData]
        public string? StatusMessage { get; set; }

        public class LogFoodInputModel
        {
            [Required]
            public int FoodItemId { get; set; }

            [Required]
            [Range(0.01, 100, ErrorMessage = "Quantity must be between 0.01 and 100.")]
            [Display(Name = "Quantity (servings)")]
            public decimal QuantityConsumed { get; set; } = 1; 

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Log Date")]
            public DateTime LoggedDate { get; set; } = DateTime.Today;

            [Required]
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "Meal")]
            public string MealContext { get; set; } = "Snack"; 
        }


        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    StatusMessage = "Error: Unable to load user.";
                    return;
                }
                var userId = await _userManager.GetUserIdAsync(user);

                var result = await _nutritionService.SearchFoodItemsAsync(SearchTerm, userId);
                if (result.IsSuccess && result.Value != null)
                {
                    SearchResults = result.Value.ToList();
                }
                else if(result.IsFailure)
                {
                    StatusMessage = $"Error searching food: {result.Error?.Message}";
                }
            }
        }
        
        public async Task<IActionResult> OnPostLogSelectedItemAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Error: User not found.";
                return Page(); 
            }
            var userId = await _userManager.GetUserIdAsync(user);

            if (!ModelState.IsValid) 
            {
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    await OnGetAsync(); 
                }
                return Page();
            }

            var logDto = new LogFoodItemRequestDto
            {
                FoodItemId = Input.FoodItemId,
                QuantityConsumed = Input.QuantityConsumed,
                LoggedDate = Input.LoggedDate,
                MealContext = Input.MealContext
            };

            var result = await _nutritionService.LogFoodAsync(logDto, userId);

            if (result.IsSuccess)
            {
                StatusMessage = $"Successfully logged '{result.Value?.FoodItemName}'.";
                return RedirectToPage(new { SearchTerm = this.SearchTerm });
            }
            else
            {
                StatusMessage = $"Error logging food: {result.Error?.Message}";
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                   await OnGetAsync();
                }
                return Page();
            }
        }
    }
}