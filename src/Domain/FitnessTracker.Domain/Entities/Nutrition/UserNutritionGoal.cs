using FitnessTracker.Domain.Enums.Nutrition;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class UserNutritionGoal
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ApplicationUser { get; set; }

        public FitnessGoalType GoalType { get; set; } = FitnessGoalType.NotSet;

        public int TargetCalories { get; set; }  
        public int TargetProteinGrams { get; set; } 
        public int TargetCarbohydratesGrams { get; set; }
        public int TargetFatGrams { get; set; }      

        public DateTime LastUpdatedDate { get; set; }

        public UserNutritionGoal()
        {
            LastUpdatedDate = DateTime.UtcNow;
        }
    }
}