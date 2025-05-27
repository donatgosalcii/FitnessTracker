using FitnessTracker.Domain.Entities.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations.Nutrition
{
    public class UserNutritionGoalConfiguration : IEntityTypeConfiguration<UserNutritionGoal>
    {
        public void Configure(EntityTypeBuilder<UserNutritionGoal> builder)
        {
            builder.ToTable("UserNutritionGoals", "Nutrition"); // Specify schema for organization

            builder.HasKey(ung => ung.Id);

            builder.HasIndex(ung => ung.ApplicationUserId).IsUnique();

            builder.Property(ung => ung.GoalType)
                .IsRequired()
                .HasConversion<string>() // Store enum as string for readability
                .HasMaxLength(50);

            builder.Property(ung => ung.TargetCalories).IsRequired();
            builder.Property(ung => ung.TargetProteinGrams).IsRequired();
            builder.Property(ung => ung.TargetCarbohydratesGrams).IsRequired();
            builder.Property(ung => ung.TargetFatGrams).IsRequired();

            builder.Property(ung => ung.LastUpdatedDate).IsRequired();

            builder.HasOne(ung => ung.ApplicationUser)
                .WithOne(au => au.NutritionGoal) // Matches the ApplicationUser.NutritionGoal navigation property
                .HasForeignKey<UserNutritionGoal>(ung => ung.ApplicationUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, their goal is also deleted.
        }
    }
}