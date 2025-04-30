using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasMany(e => e.MuscleGroups)
               .WithMany(mg => mg.Exercises);

        builder.HasMany(e => e.WorkoutSets)
               .WithOne(ws => ws.Exercise)
               .HasForeignKey(ws => ws.ExerciseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}