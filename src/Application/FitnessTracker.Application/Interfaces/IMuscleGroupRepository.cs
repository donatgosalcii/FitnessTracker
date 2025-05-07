using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public interface IMuscleGroupRepository
    {
        Task<IEnumerable<MuscleGroup>> GetAllAsync();
        Task<MuscleGroup?> GetByIdAsync(int id);
        Task<MuscleGroup?> GetByNameAsync(string name);
        Task<MuscleGroup> AddAsync(MuscleGroup muscleGroup);
        Task UpdateAsync(MuscleGroup muscleGroup);
        Task DeleteAsync(MuscleGroup muscleGroup);
    }
}