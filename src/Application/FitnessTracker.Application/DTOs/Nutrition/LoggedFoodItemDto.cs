namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class LoggedFoodItemDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;

        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; } = string.Empty;
        public string? FoodItemBrand { get; set; }

        public DateTime LoggedDate { get; set; } 
        public DateTime Timestamp { get; set; }  
        public string MealContext { get; set; } = string.Empty; 

        public decimal QuantityConsumed { get; set; }

        public decimal CalculatedCalories { get; set; }
        public decimal CalculatedProtein { get; set; }
        public decimal CalculatedCarbohydrates { get; set; }
        public decimal CalculatedFat { get; set; }
    }
}