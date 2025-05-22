using FitnessTracker.Domain.Enums.Nutrition;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class UserNutritionGoal
    {
        public int Id { get; set; } // Primary Key

        public string ApplicationUserId { get; set; } = string.Empty; // Foreign Key
        public virtual ApplicationUser ApplicationUser { get; set; } // Navigation property

        public FitnessGoalType GoalType { get; set; } = FitnessGoalType.NotSet;

        public int TargetCalories { get; set; }         // e.g., 2800 kcal
        public int TargetProteinGrams { get; set; }     // e.g., 180g
        public int TargetCarbohydratesGrams { get; set; } // e.g., 300g
        public int TargetFatGrams { get; set; }         // e.g., 80g

        public DateTime LastUpdatedDate { get; set; }

        public UserNutritionGoal()
        {
            LastUpdatedDate = DateTime.UtcNow;
        }
    }
}