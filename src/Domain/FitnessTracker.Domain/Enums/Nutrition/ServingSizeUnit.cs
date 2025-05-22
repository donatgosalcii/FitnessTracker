namespace FitnessTracker.Domain.Enums.Nutrition
{
    public enum ServingSizeUnit
    {
        g = 1,      // Grams
        ml = 2,     // Milliliters
        oz = 3,     // Ounces (fluid or weight, context dependent or specify)
        fl_oz = 4,  // Fluid Ounces
        cup = 5,
        tbsp = 6,   // Tablespoon
        tsp = 7,    // Teaspoon
        piece = 8,  // e.g., 1 apple
        slice = 9,
        serving = 10 // A generic "serving" as defined by the product
    }
}