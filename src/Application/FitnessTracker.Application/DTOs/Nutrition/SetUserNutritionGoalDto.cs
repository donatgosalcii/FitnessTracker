using FitnessTracker.Domain.Enums.Nutrition;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class SetUserNutritionGoalDto
    {
        [Required]
        public FitnessGoalType GoalType { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Target calories must be between 0 and 10,000.")]
        public int TargetCalories { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Target protein must be between 0 and 1,000 grams.")]
        public int TargetProteinGrams { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Target carbohydrates must be between 0 and 1,000 grams.")]
        public int TargetCarbohydratesGrams { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Target fat must be between 0 and 1,000 grams.")]
        public int TargetFatGrams { get; set; }
    }
}