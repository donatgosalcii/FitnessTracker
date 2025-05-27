using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities.Nutrition;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories.Nutrition
{
    public class UserNutritionGoalRepository : IUserNutritionGoalRepository
    {
        private readonly ApplicationDbContext _context;

        public UserNutritionGoalRepository(ApplicationDbContext context )
        {
            _context = context;
        }

        public async Task<UserNutritionGoal?> GetByIdAsync(int id)
        {
            return await _context.UserNutritionGoals
                                 .AsNoTracking() 
                                 .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<UserNutritionGoal?> GetByUserIdAsync(string userId)
        {
            return await _context.UserNutritionGoals
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(g => g.ApplicationUserId == userId);
        }

        public async Task<UserNutritionGoal> AddAsync(UserNutritionGoal entity)
        {
            await _context.UserNutritionGoals.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(UserNutritionGoal entity)
        {
            _context.UserNutritionGoals.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(UserNutritionGoal entity)
        {
            _context.UserNutritionGoals.Remove(entity);
            return Task.CompletedTask;
        }
    }
}