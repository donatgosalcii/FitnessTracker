using FitnessTracker.Domain.Entities.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations.Nutrition
{
    public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
    {
        public void Configure(EntityTypeBuilder<FoodItem> builder)
        {
            builder.ToTable("FoodItems", "Nutrition"); // Specify schema

            builder.HasKey(fi => fi.Id);

            builder.Property(fi => fi.Name).IsRequired().HasMaxLength(200);
            builder.Property(fi => fi.Brand).HasMaxLength(100);

            builder.Property(fi => fi.ServingSizeValue).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.ServingUnit)
                .IsRequired()
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20);

            builder.Property(fi => fi.CaloriesPerServing).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.ProteinPerServing).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.CarbohydratesPerServing).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.FatPerServing).IsRequired().HasColumnType("decimal(10,2)");

            builder.Property(fi => fi.FiberPerServing).HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.SugarPerServing).HasColumnType("decimal(10,2)");
            builder.Property(fi => fi.SodiumPerServing).HasColumnType("decimal(10,2)");

            builder.Property(fi => fi.Barcode).HasMaxLength(100);
            builder.HasIndex(fi => fi.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL"); // Unique if not null

            builder.HasOne(fi => fi.ApplicationUser)
                .WithMany(au => au.CustomFoodItems) // Matches ApplicationUser.CustomFoodItems
                .HasForeignKey(fi => fi.ApplicationUserId)
                .IsRequired(false) // Nullable: FoodItems can be global (ApplicationUserId is null) or user-specific
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, their custom food items are also deleted.
        }
    }
}