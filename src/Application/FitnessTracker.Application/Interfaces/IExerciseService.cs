using FitnessTracker.Application.Common; 
using FitnessTracker.Application.DTOs.Exercise;

namespace FitnessTracker.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<Result<IEnumerable<ExerciseDto>>> GetAllExercisesAsync();
        Task<Result<ExerciseDto>> GetExerciseByIdAsync(int id);
        Task<Result<ExerciseDto>> CreateExerciseAsync(CreateExerciseDto createDto);
        Task<Result<ExerciseDto>> UpdateExerciseAsync(int id, UpdateExerciseDto updateDto); 
        Task<Result> DeleteExerciseAsync(int id); 
    }
}