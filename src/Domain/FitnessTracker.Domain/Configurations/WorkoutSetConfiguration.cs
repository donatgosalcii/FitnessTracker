using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace FitnessTracker.Domain.Configurations;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Weight).HasColumnType("decimal(6, 2)");
        builder.Property(ws => ws.Distance).HasColumnType("decimal(8, 2)");

        builder.Property(ws => ws.Notes)
            .HasMaxLength(500);

        builder.HasIndex(ws => ws.ExerciseId);

        builder.HasOne(ws => ws.Workout)
               .WithMany(w => w.Sets)
               .HasForeignKey(ws => ws.WorkoutId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ws => ws.WorkoutId);
    }
}