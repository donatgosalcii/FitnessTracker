using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public interface IExerciseRepository
    {
        Task<IEnumerable<Exercise>> GetAllAsync();
        Task<IEnumerable<Exercise>> GetAllWithDetailsAsync();
        Task<Exercise?> GetByIdAsync(int id);
        Task<Exercise?> GetByIdWithDetailsAsync(int id);
        Task<Exercise?> GetByNameAsync(string name);
        Task<Exercise> AddAsync(Exercise exercise);
        Task UpdateAsync(Exercise exercise);
        Task DeleteAsync(Exercise exercise);
    }
}