using FitnessTracker.Domain.Entities.Nutrition;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessTracker.Application.Interfaces
{
    public interface IFoodItemRepository
    {
        Task<FoodItem?> GetByIdAsync(int id);
        Task<IEnumerable<FoodItem>> GetAllAsync();
        Task<IEnumerable<FoodItem>> GetByUserIdAsync(string userId);
        Task<IEnumerable<FoodItem>> FindAsync(System.Linq.Expressions.Expression<System.Func<FoodItem, bool>> predicate);
        Task<FoodItem> AddAsync(FoodItem entity);
        Task UpdateAsync(FoodItem entity);
        Task DeleteAsync(FoodItem entity); 

        Task<IEnumerable<FoodItem>> SearchByNameAsync(string nameQuery, string? userId = null);
    }
}