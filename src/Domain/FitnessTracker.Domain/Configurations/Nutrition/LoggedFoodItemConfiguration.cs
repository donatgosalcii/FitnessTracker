using FitnessTracker.Domain.Entities.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations.Nutrition
{
    public class LoggedFoodItemConfiguration : IEntityTypeConfiguration<LoggedFoodItem>
    {
        public void Configure(EntityTypeBuilder<LoggedFoodItem> builder)
        {
            builder.ToTable("LoggedFoodItems", "Nutrition"); // Specify schema

            builder.HasKey(lfi => lfi.Id);

            builder.Property(lfi => lfi.LoggedDate).IsRequired().HasColumnType("date"); // Store as date only for daily grouping
            builder.Property(lfi => lfi.Timestamp).IsRequired(); // Actual time of logging/consumption
            builder.Property(lfi => lfi.MealContext).IsRequired().HasMaxLength(50); // e.g., "Breakfast", "Lunch"
            builder.Property(lfi => lfi.QuantityConsumed).IsRequired().HasColumnType("decimal(8,2)");

            builder.Property(lfi => lfi.CalculatedCalories).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(lfi => lfi.CalculatedProtein).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(lfi => lfi.CalculatedCarbohydrates).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(lfi => lfi.CalculatedFat).IsRequired().HasColumnType("decimal(10,2)");

            builder.HasOne(lfi => lfi.ApplicationUser)
                .WithMany(au => au.LoggedFoodItems) // Matches ApplicationUser.LoggedFoodItems
                .HasForeignKey(lfi => lfi.ApplicationUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, their food log is deleted.

            builder.HasOne(lfi => lfi.FoodItem)
                .WithMany(fi => fi.LoggedFoodItems) // Matches FoodItem.LoggedFoodItems
                .HasForeignKey(lfi => lfi.FoodItemId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of a FoodItem if it has been logged.
        }
    }
}