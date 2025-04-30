using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.DatePerformed)
            .IsRequired();

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        builder.HasMany(w => w.Sets)
               .WithOne(ws => ws.Workout)
               .HasForeignKey(ws => ws.WorkoutId);

    }
}