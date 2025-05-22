using System;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class LoggedFoodItem
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty; // User who logged this
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int FoodItemId { get; set; } // FK to FoodItem
        public virtual FoodItem FoodItem { get; set; }

        public DateTime LoggedDate { get; set; } // The date this food was consumed for
        public DateTime Timestamp { get; set; }    // Actual time it was logged or eaten

        public string MealContext { get; set; } = "Snack";

        public decimal QuantityConsumed { get; set; } // e.g., 1.5 (meaning 1.5 x FoodItem.ServingSize)

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