using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations
{
    public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.HasKey(we => we.Id);

            builder.Property(we => we.Weight)
                .HasColumnType("decimal(6, 2)")
                .IsRequired();

            builder.HasOne(we => we.Exercise)
                .WithMany()
                .HasForeignKey(we => we.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(we => we.Workout)
                .WithMany()
                .HasForeignKey(we => we.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(we => we.ExerciseId);
            builder.HasIndex(we => we.WorkoutId);
        }
    }
} 