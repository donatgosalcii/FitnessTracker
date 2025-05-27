using FitnessTracker.Domain.Enums.Nutrition;

namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class UserNutritionGoalDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;

        public FitnessGoalType GoalType { get; set; }
        public string GoalTypeDescription => GoalType.ToString();

        public int TargetCalories { get; set; }
        public int TargetProteinGrams { get; set; }
        public int TargetCarbohydratesGrams { get; set; }
        public int TargetFatGrams { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }
}