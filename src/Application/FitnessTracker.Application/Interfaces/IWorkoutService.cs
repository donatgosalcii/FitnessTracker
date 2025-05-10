using FitnessTracker.Application.DTOs.Workout;

namespace FitnessTracker.Application.Interfaces
{
    public interface IWorkoutService
    {
        Task<IEnumerable<WorkoutSummaryDto>> GetUserWorkoutsAsync(string userId);
        Task<WorkoutDetailDto?> GetWorkoutDetailsAsync(int workoutId, string userId);
        Task<WorkoutDetailDto> LogWorkoutAsync(LogWorkoutDto logDto, string userId);
        Task<bool> UpdateWorkoutAsync(int workoutId, string userId, UpdateWorkoutDto updateDto);
        Task<bool> DeleteWorkoutAsync(int workoutId, string userId);
    }
}