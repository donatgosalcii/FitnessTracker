using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.MuscleGroup;

namespace FitnessTracker.Application.Interfaces
{
    public interface IMuscleGroupService
    {
        Task<Result<IEnumerable<MuscleGroupDto>>> GetAllAsync();
        Task<Result<MuscleGroupDto>> GetByIdAsync(int id); 
        Task<Result<MuscleGroupDto>> CreateAsync(CreateMuscleGroupDto createDto);
        Task<Result> UpdateAsync(int id, UpdateMuscleGroupDto updateDto); 
        Task<Result> DeleteAsync(int id);
    }
}