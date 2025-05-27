using FitnessTracker.Domain.Enums.Nutrition;

namespace FitnessTracker.Domain.Entities.Nutrition
{
    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }

        public decimal ServingSizeValue { get; set; } 
        public ServingSizeUnit ServingUnit { get; set; }

        public decimal CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; } 
        public decimal CarbohydratesPerServing { get; set; }
        public decimal FatPerServing { get; set; }

        public decimal? FiberPerServing { get; set; }
        public decimal? SugarPerServing { get; set; }
        public decimal? SodiumPerServing { get; set; }

        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; } 

        public string? Barcode { get; set; }

        public virtual ICollection<LoggedFoodItem> LoggedFoodItems { get; set; } = new List<LoggedFoodItem>();
    }
}