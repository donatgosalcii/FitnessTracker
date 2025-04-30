using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations;

public class MuscleGroupConfiguration : IEntityTypeConfiguration<MuscleGroup>
{
    public void Configure(EntityTypeBuilder<MuscleGroup> builder)
    {
        builder.HasKey(mg => mg.Id);

        builder.Property(mg => mg.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(mg => mg.Exercises)
               .WithMany(e => e.MuscleGroups);

        builder.HasData(
            new MuscleGroup { Id = 1, Name = "Chest" },
            new MuscleGroup { Id = 2, Name = "Back" },
            new MuscleGroup { Id = 3, Name = "Legs" },
            new MuscleGroup { Id = 4, Name = "Shoulders" },
            new MuscleGroup { Id = 5, Name = "Biceps" },
            new MuscleGroup { Id = 6, Name = "Triceps" },
            new MuscleGroup { Id = 7, Name = "Abs" },
            new MuscleGroup { Id = 8, Name = "Cardio" }
        );
    }
}