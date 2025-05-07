using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

public class MuscleGroupRepository : IMuscleGroupRepository
{
    private readonly ApplicationDbContext _context;

    public MuscleGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MuscleGroup> AddAsync(MuscleGroup muscleGroup)
    {
        await _context.MuscleGroups.AddAsync(muscleGroup);
        return muscleGroup; 
    }

    public async Task DeleteAsync(MuscleGroup muscleGroup)
    {
        _context.MuscleGroups.Remove(muscleGroup);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<MuscleGroup>> GetAllAsync()
    {
        return await _context.MuscleGroups
            .OrderBy(mg => mg.Name)
            .ToListAsync();
    }

    public async Task<MuscleGroup?> GetByIdAsync(int id)
    {
        return await _context.MuscleGroups.FindAsync(id);
    }

    public async Task<MuscleGroup?> GetByNameAsync(string name)
    {
        return await _context.MuscleGroups
            .FirstOrDefaultAsync(mg => mg.Name.ToLower() == name.ToLower());
    }

    public async Task UpdateAsync(MuscleGroup muscleGroup)
    {
        _context.MuscleGroups.Update(muscleGroup);
        await Task.CompletedTask;
    }
}