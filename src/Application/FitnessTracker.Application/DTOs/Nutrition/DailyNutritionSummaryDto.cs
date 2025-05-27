namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class DailyNutritionSummaryDto
    {
        public DateTime Date { get; set; }

        public decimal TotalCaloriesConsumed { get; set; }
        public decimal TotalProteinConsumed { get; set; }
        public decimal TotalCarbohydratesConsumed { get; set; }
        public decimal TotalFatConsumed { get; set; }

        public UserNutritionGoalDto? Goal { get; set; }

        public decimal CaloriesRemaining => (Goal?.TargetCalories ?? 0) - TotalCaloriesConsumed;
        public decimal ProteinRemaining => (Goal?.TargetProteinGrams ?? 0) - TotalProteinConsumed;
        public decimal CarbohydratesRemaining => (Goal?.TargetCarbohydratesGrams ?? 0) - TotalCarbohydratesConsumed;
        public decimal FatRemaining => (Goal?.TargetFatGrams ?? 0) - TotalFatConsumed;

        public List<LoggedFoodItemDto> LoggedItems { get; set; } = new List<LoggedFoodItemDto>();

        public DailyNutritionSummaryDto(DateTime date)
        {
            Date = date;
        }
    }
}