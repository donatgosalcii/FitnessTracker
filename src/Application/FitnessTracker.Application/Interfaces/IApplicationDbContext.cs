using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<MuscleGroup> MuscleGroups { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutSet> WorkoutSets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}