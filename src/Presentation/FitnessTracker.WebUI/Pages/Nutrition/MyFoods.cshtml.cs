using FitnessTracker.Application.DTOs.Nutrition;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities; 
using FitnessTracker.Domain.Enums.Nutrition; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.WebUI.Pages.Nutrition
{
    [Authorize]
    public class MyFoodsModel : PageModel
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyFoodsModel(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        public IList<FoodItemDto> UserFoodItems { get; set; } = new List<FoodItemDto>();

        [BindProperty]
        public AddFoodItemInputModel NewFoodItem { get; set; } = new AddFoodItemInputModel();

        public SelectList ServingUnitSelectList { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class AddFoodItemInputModel
        {
            [Required]
            [StringLength(200, MinimumLength = 2)]
            [Display(Name = "Food Name")]
            public string Name { get; set; } = string.Empty;

            [StringLength(100)]
            public string? Brand { get; set; }

            [Required]
            [Range(0.01, 10000)]
            [Display(Name = "Serving Size")]
            public decimal ServingSizeValue { get; set; }

            [Required]
            [Display(Name = "Serving Unit")]
            public ServingSizeUnit ServingUnit { get; set; }

            [Required]
            [Range(0, 10000)]
            [Display(Name = "Calories (per serving)")]
            public decimal CaloriesPerServing { get; set; }

            [Required]
            [Range(0, 1000)]
            [Display(Name = "Protein (g per serving)")]
            public decimal ProteinPerServing { get; set; }

            [Required]
            [Range(0, 1000)]
            [Display(Name = "Carbs (g per serving)")]
            public decimal CarbohydratesPerServing { get; set; }

            [Required]
            [Range(0, 1000)]
            [Display(Name = "Fat (g per serving)")]
            public decimal FatPerServing { get; set; }

            [Range(0, 1000)]
            [Display(Name = "Fiber (g per serving)")]
            public decimal? FiberPerServing { get; set; }

            [Range(0, 1000)]
            [Display(Name = "Sugar (g per serving)")]
            public decimal? SugarPerServing { get; set; }

            [Range(0, 50000)]
            [Display(Name = "Sodium (mg per serving)")]
            public decimal? SodiumPerServing { get; set; }
            
            [StringLength(100)]
            public string? Barcode { get; set; }
        }

        private async Task LoadUserFoodItemsAsync(string userId)
        {
            var result = await _nutritionService.GetUserCustomFoodItemsAsync(userId);
            if (result.IsSuccess && result.Value != null)
            {
                UserFoodItems = result.Value.ToList();
            }
            else if(result.IsFailure)
            {
                StatusMessage = $"Error loading food items: {result.Error?.Message}"; 
                UserFoodItems = new List<FoodItemDto>(); 
            }
            else
            {
                 UserFoodItems = new List<FoodItemDto>(); 
            }
        }

        private void PopulateServingUnitSelectList(ServingSizeUnit selectedUnit = ServingSizeUnit.g)
        {
            var enumValues = Enum.GetValues(typeof(ServingSizeUnit))
                                .Cast<ServingSizeUnit>()
                                .Select(e => new SelectListItem
                                {
                                    Value = ((int)e).ToString(),
                                    Text = e.ToString() 
                                });
            ServingUnitSelectList = new SelectList(enumValues, "Value", "Text", (int)selectedUnit);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user.");
            
            var userId = await _userManager.GetUserIdAsync(user);
            await LoadUserFoodItemsAsync(userId);
            PopulateServingUnitSelectList(NewFoodItem?.ServingUnit ?? ServingSizeUnit.g); 

            return Page();
        }

        public async Task<IActionResult> OnPostAddFoodItemAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user.");
            var userId = await _userManager.GetUserIdAsync(user);

            PopulateServingUnitSelectList(NewFoodItem.ServingUnit);

            if (!ModelState.IsValid)
            {
                await LoadUserFoodItemsAsync(userId); 
                return Page();
            }

            var createDto = new CreateFoodItemDto
            {
                Name = NewFoodItem.Name,
                Brand = NewFoodItem.Brand,
                ServingSizeValue = NewFoodItem.ServingSizeValue,
                ServingUnit = NewFoodItem.ServingUnit,
                CaloriesPerServing = NewFoodItem.CaloriesPerServing,
                ProteinPerServing = NewFoodItem.ProteinPerServing,
                CarbohydratesPerServing = NewFoodItem.CarbohydratesPerServing,
                FatPerServing = NewFoodItem.FatPerServing,
                FiberPerServing = NewFoodItem.FiberPerServing,
                SugarPerServing = NewFoodItem.SugarPerServing,
                SodiumPerServing = NewFoodItem.SodiumPerServing,
                Barcode = NewFoodItem.Barcode
            };

            var result = await _nutritionService.CreateFoodItemAsync(createDto, userId);

            if (result.IsSuccess)
            {
                StatusMessage = $"Food item '{result.Value?.Name}' added successfully.";
                return RedirectToPage(); 
            }
            else
            {
                StatusMessage = $"Error adding food item: {result.Error?.Message} (Code: {result.Error?.Code})";
                await LoadUserFoodItemsAsync(userId); 
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteFoodItemAsync(int foodItemId) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                StatusMessage = "Error: User not found or not authenticated.";
                return RedirectToPage("./MyFoods");
            }
            var userId = await _userManager.GetUserIdAsync(user);

            var result = await _nutritionService.DeleteFoodItemAsync(foodItemId, userId);
            
            if (result.IsSuccess)
            {
                StatusMessage = "Food item deleted successfully.";
            }
            else
            {
                StatusMessage = $"Error deleting food item: {result.Error?.Message ?? "An unknown error occurred."} (Code: {result.Error?.Code})";
            }
            return RedirectToPage(); 
        }
    }
}