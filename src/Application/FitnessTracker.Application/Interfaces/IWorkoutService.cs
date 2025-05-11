using FitnessTracker.Application.Common; 
using FitnessTracker.Application.DTOs.Workout;

namespace FitnessTracker.Application.Interfaces
{
    public interface IWorkoutService
    {
        Task<Result<IEnumerable<WorkoutSummaryDto>>> GetUserWorkoutsAsync(string userId);
        Task<Result<WorkoutDetailDto>> GetWorkoutDetailsAsync(int workoutId, string userId);
        Task<Result<WorkoutDetailDto>> LogWorkoutAsync(LogWorkoutDto logDto, string userId);
        Task<Result<WorkoutDetailDto>> UpdateWorkoutAsync(int workoutId, string userId, UpdateWorkoutDto updateDto); 
        Task<Result> DeleteWorkoutAsync(int workoutId, string userId); 
    }
}