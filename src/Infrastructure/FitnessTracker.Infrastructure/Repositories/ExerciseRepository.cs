using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExerciseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Exercise> AddAsync(Exercise exercise)
        {
            await _context.Exercises.AddAsync(exercise);
            return exercise;
        }

        public async Task DeleteAsync(Exercise exercise)
        {
            _context.Exercises.Remove(exercise);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Exercise>> GetAllAsync()
        {
            return await _context.Exercises
                                 .OrderBy(e => e.Name)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> GetAllWithDetailsAsync()
        {
            return await _context.Exercises
                                 .Include(e => e.MuscleGroups) 
                                 .OrderBy(e => e.Name)
                                 .ToListAsync();
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises.FindAsync(id);
        }

        public async Task<Exercise?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Exercises
                                 .Include(e => e.MuscleGroups) 
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Exercise?> GetByNameAsync(string name)
        {
            return await _context.Exercises
                                 .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
        }

        public async Task UpdateAsync(Exercise exercise)
        {
            _context.Exercises.Update(exercise);
            await Task.CompletedTask;
        }
    }
}