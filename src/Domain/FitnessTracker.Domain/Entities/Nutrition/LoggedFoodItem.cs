using System;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class LoggedFoodItem
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int FoodItemId { get; set; }
        public virtual FoodItem FoodItem { get; set; }

        public DateTime LoggedDate { get; set; }
        public DateTime Timestamp { get; set; } 

        public string MealContext { get; set; } = "Snack";

        public decimal QuantityConsumed { get; set; } 

        public decimal CalculatedCalories { get; set; }
        public decimal CalculatedProtein { get; set; }
        public decimal CalculatedCarbohydrates { get; set; }
        public decimal CalculatedFat { get; set; }

        public LoggedFoodItem()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}