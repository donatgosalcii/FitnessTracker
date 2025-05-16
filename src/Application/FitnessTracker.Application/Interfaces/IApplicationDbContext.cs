using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<MuscleGroup> MuscleGroups { get; set; }
    DbSet<Exercise> Exercises { get; set; }
    DbSet<Workout> Workouts { get; set; }
    DbSet<WorkoutSet> WorkoutSets { get; }
    DbSet<WorkoutExercise> WorkoutExercises { get; set; }
    DbSet<ChatMessage> ChatMessages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}