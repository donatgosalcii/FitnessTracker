using FitnessTracker.Application.DTOs.Exercise;

namespace FitnessTracker.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync();
        Task<ExerciseDto?> GetExerciseByIdAsync(int id);
        Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto createDto);
        Task<ExerciseDto?> UpdateExerciseAsync(int id, UpdateExerciseDto updateDto); 
        Task<bool> DeleteExerciseAsync(int id);
    }
}