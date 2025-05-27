using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class LogFoodItemRequestDto
    {
        [Required]
        public int FoodItemId { get; set; } 

        [Required]
        [Range(0.01, 100)]
        public decimal QuantityConsumed { get; set; }

        [Required]
        public DateTime LoggedDate { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string MealContext { get; set; } = string.Empty; 
    }
}