using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public interface IWorkoutRepository
    {
        Task<IEnumerable<Workout>> GetWorkoutsForUserAsync(string userId);

        Task<Workout?> GetWorkoutByIdWithDetailsAsync(int workoutId, string userId);

        Task<Workout?> GetWorkoutByIdAsync(int workoutId, string userId);

        Task<Workout> AddAsync(Workout workout);

        Task DeleteAsync(Workout workout);

        Task UpdateAsync(Workout workout);
    }
}