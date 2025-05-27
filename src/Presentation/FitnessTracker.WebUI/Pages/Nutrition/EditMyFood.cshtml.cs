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
    public class EditMyFoodModel : PageModel
    {
        private readonly INutritionService _nutritionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditMyFoodModel(INutritionService nutritionService, UserManager<ApplicationUser> userManager)
        {
            _nutritionService = nutritionService;
            _userManager = userManager;
        }

        [BindProperty]
        public FoodItemInputModel Input { get; set; } = new FoodItemInputModel();

        public SelectList ServingUnitSelectList { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class FoodItemInputModel 
        {
            [HiddenInput]
            public int Id { get; set; } 

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
        
        private void PopulateServingUnitSelectList(ServingSizeUnit selectedUnit)
        {
            var enumValues = Enum.GetValues(typeof(ServingSizeUnit))
                                .Cast<ServingSizeUnit>()
                                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });
            ServingUnitSelectList = new SelectList(enumValues, "Value", "Text", (int)selectedUnit);
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                StatusMessage = "Error: Food item ID not provided.";
                return RedirectToPage("./MyFoods");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                 StatusMessage = "Error: User not found.";
                 return RedirectToPage("/Account/Login");
            }
            var userId = await _userManager.GetUserIdAsync(user);

            var result = await _nutritionService.GetFoodItemByIdAsync(id.Value);

            if (!result.IsSuccess || result.Value == null)
            {
                StatusMessage = $"Error: Food item with ID {id.Value} not found or error loading. {result.Error?.Message}";
                return RedirectToPage("./MyFoods");
            }

            var foodItemDto = result.Value;

            if (foodItemDto.ApplicationUserId != null && foodItemDto.ApplicationUserId != userId)
            {
                StatusMessage = "Error: You are not authorized to edit this food item.";
                return RedirectToPage("./MyFoods");
            }
            
            Input = new FoodItemInputModel
            {
                Id = foodItemDto.Id,
                Name = foodItemDto.Name,
                Brand = foodItemDto.Brand,
                ServingSizeValue = foodItemDto.ServingSizeValue,
                ServingUnit = foodItemDto.ServingUnit,
                CaloriesPerServing = foodItemDto.CaloriesPerServing,
                ProteinPerServing = foodItemDto.ProteinPerServing,
                CarbohydratesPerServing = foodItemDto.CarbohydratesPerServing,
                FatPerServing = foodItemDto.FatPerServing,
                FiberPerServing = foodItemDto.FiberPerServing,
                SugarPerServing = foodItemDto.SugarPerServing,
                SodiumPerServing = foodItemDto.SodiumPerServing,
                Barcode = foodItemDto.Barcode
            };
            
            PopulateServingUnitSelectList(Input.ServingUnit);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PopulateServingUnitSelectList(Input.ServingUnit);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                StatusMessage = "Error: User not found.";
                return Page();
            }
            var userId = await _userManager.GetUserIdAsync(user);

            if (Input.Id == 0) 
            {
                ModelState.AddModelError(string.Empty, "Food item ID is missing for update.");
                return Page();
            }

            var updateDto = new UpdateFoodItemDto 
            {
                Name = Input.Name,
                Brand = Input.Brand,
                ServingSizeValue = Input.ServingSizeValue,
                ServingUnit = Input.ServingUnit,
                CaloriesPerServing = Input.CaloriesPerServing,
                ProteinPerServing = Input.ProteinPerServing,
                CarbohydratesPerServing = Input.CarbohydratesPerServing,
                FatPerServing = Input.FatPerServing,
                FiberPerServing = Input.FiberPerServing,
                SugarPerServing = Input.SugarPerServing,
                SodiumPerServing = Input.SodiumPerServing,
                Barcode = Input.Barcode
            };

            var result = await _nutritionService.UpdateFoodItemAsync(Input.Id, updateDto, userId);

            if (result.IsSuccess)
            {
                StatusMessage = $"Food item '{result.Value?.Name}' updated successfully.";
                return RedirectToPage("./MyFoods");
            }
            else
            {
                StatusMessage = $"Error updating food item: {result.Error?.Message} (Code: {result.Error?.Code})";
                return Page();
            }
        }
    }
}