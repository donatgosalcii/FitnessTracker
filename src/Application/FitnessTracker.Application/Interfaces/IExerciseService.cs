// FitnessTracker.Application/Interfaces/IExerciseService.cs
using FitnessTracker.Application.Common; // For Result
using FitnessTracker.Application.DTOs.Exercise;
using System.Collections.Generic;
using System.Threading.Tasks;

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