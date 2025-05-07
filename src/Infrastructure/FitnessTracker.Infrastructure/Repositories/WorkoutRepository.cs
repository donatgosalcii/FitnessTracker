using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkoutRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Workout> AddAsync(Workout workout)
        {
            await _context.Workouts.AddAsync(workout);
            return workout;
        }

        public async Task DeleteAsync(Workout workout)
        {
            _context.Workouts.Remove(workout);
            await Task.CompletedTask; 
        }

        public async Task<IEnumerable<Workout>> GetWorkoutsForUserAsync(string userId)
        {
            return await _context.Workouts
                                 .Where(w => w.UserId == userId)
                                 .OrderByDescending(w => w.DatePerformed) 
                                 .ToListAsync();
        }

        public async Task<Workout?> GetWorkoutByIdAsync(int workoutId, string userId)
        {
            return await _context.Workouts
                                 .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);
        }

        public async Task<Workout?> GetWorkoutByIdWithDetailsAsync(int workoutId, string userId)
        {
            return await _context.Workouts
                                 .Where(w => w.Id == workoutId && w.UserId == userId)
                                 .Include(w => w.Sets) 
                                     .ThenInclude(s => s.Exercise)
                                 .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Workout workout)
        {
            _context.Entry(workout).State = EntityState.Modified;
            await Task.CompletedTask; 
        }
    }
}