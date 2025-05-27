using FitnessTracker.Domain.Enums.Nutrition; // For ServingSizeUnit
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class CreateFoodItemDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal ServingSizeValue { get; set; }

        [Required]
        public ServingSizeUnit ServingUnit { get; set; }

        [Required]
        [Range(0, 10000)] // Calories can be 0 for some items (e.g., water)
        public decimal CaloriesPerServing { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal ProteinPerServing { get; set; } // Grams

        [Required]
        [Range(0, 1000)]
        public decimal CarbohydratesPerServing { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal FatPerServing { get; set; }

        [Range(0, 1000)]
        public decimal? FiberPerServing { get; set; }

        [Range(0, 1000)]
        public decimal? SugarPerServing { get; set; }

        [Range(0, 50000)]
        public decimal? SodiumPerServing { get; set; }

        [StringLength(100)]
        public string? Barcode { get; set; }

    }
}