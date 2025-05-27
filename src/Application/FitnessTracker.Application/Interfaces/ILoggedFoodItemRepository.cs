using FitnessTracker.Domain.Entities.Nutrition;

namespace FitnessTracker.Application.Interfaces
{
    public interface ILoggedFoodItemRepository 
    {
        Task<LoggedFoodItem?> GetByIdAsync(int id);
        Task<IEnumerable<LoggedFoodItem>> GetByUserIdAndDateAsync(string userId, DateTime date);
        Task<IEnumerable<LoggedFoodItem>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<LoggedFoodItem> AddAsync(LoggedFoodItem entity);
        Task UpdateAsync(LoggedFoodItem entity);
        Task DeleteAsync(LoggedFoodItem entity);
        Task<IEnumerable<LoggedFoodItem>> FindAsync(System.Linq.Expressions.Expression<System.Func<LoggedFoodItem, bool>> predicate);
    }
}