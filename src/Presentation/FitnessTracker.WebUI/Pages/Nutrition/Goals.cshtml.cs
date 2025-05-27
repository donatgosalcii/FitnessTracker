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
    public class GoalsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INutritionService _nutritionService; 

        public GoalsModel(
            UserManager<ApplicationUser> userManager,
            INutritionService nutritionService,
            ILogger<GoalsModel> logger) 
        {
            _userManager = userManager;
            _nutritionService = nutritionService;
        }

        [BindProperty]
        public GoalInputModel Input { get; set; } = new GoalInputModel();
        public SelectList FitnessGoalTypesSelectList { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        
        public class GoalInputModel { 
            [Required] [Display(Name = "Fitness Goal")] public FitnessGoalType GoalType { get; set; }
            [Required] [Range(0, 10000)] [Display(Name = "Target Daily Calories (kcal)")] public int TargetCalories { get; set; }
            [Required] [Range(0, 1000)] [Display(Name = "Target Daily Protein (g)")] public int TargetProteinGrams { get; set; }
            [Required] [Range(0, 1000)] [Display(Name = "Target Daily Carbohydrates (g)")] public int TargetCarbohydratesGrams { get; set; }
            [Required] [Range(0, 1000)] [Display(Name = "Target Daily Fat (g)")] public int TargetFatGrams { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _nutritionService.GetUserNutritionGoalAsync(userId);

            UserNutritionGoalDto? goalDto = null;
            if (result.IsSuccess) { goalDto = result.GetValueOrDefault(); }
            
            if (goalDto != null) {Input = new GoalInputModel { GoalType = goalDto.GoalType, TargetCalories = goalDto.TargetCalories, TargetProteinGrams = goalDto.TargetProteinGrams, TargetCarbohydratesGrams = goalDto.TargetCarbohydratesGrams, TargetFatGrams = goalDto.TargetFatGrams }; }
            else {Input = new GoalInputModel { GoalType = FitnessGoalType.Maintaining, TargetCalories = 2000, TargetProteinGrams = 100, TargetCarbohydratesGrams = 250, TargetFatGrams = 60 }; if (result.IsFailure && result.Error != null) { StatusMessage = $"Error loading initial goal: {result.Error.Message}";}}
            
            var enumValues = Enum.GetValues(typeof(FitnessGoalType)).Cast<FitnessGoalType>()
                                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });
            FitnessGoalTypesSelectList = new SelectList(enumValues, "Value", "Text", (int)(Input?.GoalType ?? FitnessGoalType.Maintaining));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound($"Unable to load user."); }
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound($"Unable to load user."); }

            var enumValues = Enum.GetValues(typeof(FitnessGoalType)).Cast<FitnessGoalType>()
                                .Select(e => new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() });
            FitnessGoalTypesSelectList = new SelectList(enumValues, "Value", "Text", (int)Input.GoalType);

            if (!ModelState.IsValid) { return Page(); }

            var userId = await _userManager.GetUserIdAsync(user);
            var goalToSetDto = new SetUserNutritionGoalDto
            {
                GoalType = Input.GoalType,
                TargetCalories = Input.TargetCalories,
                TargetProteinGrams = Input.TargetProteinGrams,
                TargetCarbohydratesGrams = Input.TargetCarbohydratesGrams,
                TargetFatGrams = Input.TargetFatGrams
            };
            
            var result = await _nutritionService.SetUserNutritionGoalAsync(goalToSetDto, userId);

            if (result.IsSuccess)
            {
                StatusMessage = "Your nutrition goals have been updated.";
                return RedirectToPage();
            }
            else
            {
                StatusMessage = $"Error updating goals: {result.Error?.Message ?? "Unknown error"} (Code: {result.Error?.Code})";
                if (result.ErrorType == FitnessTracker.Application.Common.ErrorType.Validation && result.Error != null)
                {
                    ModelState.AddModelError(string.Empty, result.Error.Message);
                }
                return Page();
            }
        }
    }
}