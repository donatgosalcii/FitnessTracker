// src/Domain/FitnessTracker.Domain/Entities/Nutrition/FoodItem.cs
using FitnessTracker.Domain.Enums.Nutrition;
using System.Collections.Generic;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }

        public decimal ServingSizeValue { get; set; } // e.g., 100
        public ServingSizeUnit ServingUnit { get; set; } // e.g., g, ml

        public decimal CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; }      // in grams
        public decimal CarbohydratesPerServing { get; set; } // in grams
        public decimal FatPerServing { get; set; }          // in grams

        public decimal? FiberPerServing { get; set; }
        public decimal? SugarPerServing { get; set; }
        public decimal? SodiumPerServing { get; set; } // in mg

        // Who created this item? Null for global items, UserId for user-specific items.
        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; } // User who created this custom food

        // For future barcode scanning
        public string? Barcode { get; set; }

        public virtual ICollection<LoggedFoodItem> LoggedFoodItems { get; set; } = new List<LoggedFoodItem>();
    }
}