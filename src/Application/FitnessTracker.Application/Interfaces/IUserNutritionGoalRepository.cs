using FitnessTracker.Domain.Entities.Nutrition;

namespace FitnessTracker.Application.Interfaces
{
    public interface IUserNutritionGoalRepository
    {
        Task<UserNutritionGoal?> GetByIdAsync(int id);
        Task<UserNutritionGoal?> GetByUserIdAsync(string userId);
        Task<UserNutritionGoal> AddAsync(UserNutritionGoal entity);
        Task UpdateAsync(UserNutritionGoal entity);
        Task DeleteAsync(UserNutritionGoal entity);
    }
}