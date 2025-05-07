using FitnessTracker.Application.DTOs.MuscleGroup;

namespace FitnessTracker.Application.Interfaces
{
    public interface IMuscleGroupService
    {
        Task<IEnumerable<MuscleGroupDto>> GetAllAsync();
        Task<MuscleGroupDto?> GetByIdAsync(int id);
        Task<MuscleGroupDto> CreateAsync(CreateMuscleGroupDto createDto);
        Task<bool> UpdateAsync(int id, UpdateMuscleGroupDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}