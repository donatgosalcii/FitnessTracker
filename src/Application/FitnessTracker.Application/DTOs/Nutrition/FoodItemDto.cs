using FitnessTracker.Domain.Enums.Nutrition;

namespace FitnessTracker.Application.DTOs.Nutrition
{
    public class FoodItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }

        public decimal ServingSizeValue { get; set; }
        public ServingSizeUnit ServingUnit { get; set; }
        public string ServingUnitDescription => ServingUnit.ToString();

        public decimal CaloriesPerServing { get; set; }
        public decimal ProteinPerServing { get; set; }
        public decimal CarbohydratesPerServing { get; set; }
        public decimal FatPerServing { get; set; }

        public decimal? FiberPerServing { get; set; }
        public decimal? SugarPerServing { get; set; }
        public decimal? SodiumPerServing { get; set; }

        public string? ApplicationUserId { get; set; }
        public string? Barcode { get; set; }
    }
}