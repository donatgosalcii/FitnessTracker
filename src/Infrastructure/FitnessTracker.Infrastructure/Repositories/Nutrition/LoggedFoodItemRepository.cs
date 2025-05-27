using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities.Nutrition;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FitnessTracker.Infrastructure.Repositories.Nutrition
{
    public class LoggedFoodItemRepository : ILoggedFoodItemRepository
    {
        private readonly ApplicationDbContext _context;

        public LoggedFoodItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoggedFoodItem?> GetByIdAsync(int id)
        {
            return await _context.LoggedFoodItems
                                 .Include(lfi => lfi.FoodItem)
                                 .FirstOrDefaultAsync(lfi => lfi.Id == id);
        }

        public async Task<IEnumerable<LoggedFoodItem>> GetByUserIdAndDateAsync(string userId, DateTime date)
        {
            var targetDate = date.Date;
            return await _context.LoggedFoodItems
                                 .Include(lfi => lfi.FoodItem)
                                 .Where(lfi => lfi.ApplicationUserId == userId && lfi.LoggedDate == targetDate)
                                 .OrderBy(lfi => lfi.Timestamp)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<LoggedFoodItem>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var sDate = startDate.Date;
            var eDate = endDate.Date.AddDays(1).AddTicks(-1); 

            return await _context.LoggedFoodItems
                                 .Include(lfi => lfi.FoodItem)
                                 .Where(lfi => lfi.ApplicationUserId == userId && lfi.LoggedDate >= sDate && lfi.LoggedDate <= eDate)
                                 .OrderBy(lfi => lfi.LoggedDate)
                                 .ThenBy(lfi => lfi.Timestamp)
                                 .ToListAsync();
        }

        public async Task<LoggedFoodItem> AddAsync(LoggedFoodItem entity)
        {
            await _context.LoggedFoodItems.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(LoggedFoodItem entity)
        {
            _context.LoggedFoodItems.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(LoggedFoodItem entity)
        {
            _context.LoggedFoodItems.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<LoggedFoodItem>> FindAsync(Expression<Func<LoggedFoodItem, bool>> predicate)
        {
             return await _context.LoggedFoodItems
                                 .Include(lfi => lfi.FoodItem)
                                 .Where(predicate)
                                 .ToListAsync();
        }
    }
}