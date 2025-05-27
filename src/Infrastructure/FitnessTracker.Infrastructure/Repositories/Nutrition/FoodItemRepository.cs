using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities.Nutrition;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FitnessTracker.Infrastructure.Repositories.Nutrition
{
    public class FoodItemRepository : IFoodItemRepository
    {
        private readonly ApplicationDbContext _context;

        public FoodItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FoodItem?> GetByIdAsync(int id)
        {
            return await _context.FoodItems.FindAsync(id);
        }

        public async Task<IEnumerable<FoodItem>> GetAllAsync()
        {
            return await _context.FoodItems
                                 .Where(fi => fi.ApplicationUserId == null) 
                                 .ToListAsync();
        }

        public async Task<IEnumerable<FoodItem>> GetByUserIdAsync(string userId)
        {
            return await _context.FoodItems
                                 .Where(fi => fi.ApplicationUserId == userId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<FoodItem>> FindAsync(Expression<Func<FoodItem, bool>> predicate)
        {
            return await _context.FoodItems.Where(predicate).ToListAsync();
        }

        public async Task<FoodItem> AddAsync(FoodItem entity)
        {
            await _context.FoodItems.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(FoodItem entity)
        {
            _context.FoodItems.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(FoodItem entity)
        {
            _context.FoodItems.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<FoodItem>> SearchByNameAsync(string nameQuery, string? userId = null)
        {
            var query = _context.FoodItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nameQuery))
            {
                query = query.Where(fi => fi.Name.Contains(nameQuery));
            }

            if (userId != null)
            {
                query = query.Where(fi => fi.ApplicationUserId == userId || fi.ApplicationUserId == null);
            }
            else
            {
                query = query.Where(fi => fi.ApplicationUserId == null);
            }

            return await query.OrderBy(fi => fi.Name).ToListAsync();
        }
    }
}