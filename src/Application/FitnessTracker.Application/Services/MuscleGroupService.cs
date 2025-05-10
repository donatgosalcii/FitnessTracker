using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly IMuscleGroupRepository _muscleGroupRepository;
    private readonly IApplicationDbContext _dbContext;

    public MuscleGroupService(
        IMuscleGroupRepository muscleGroupRepository,
        IApplicationDbContext dbContext)
    {
        _muscleGroupRepository = muscleGroupRepository;
        _dbContext = dbContext;
    }

    public async Task<MuscleGroupDto> CreateAsync(CreateMuscleGroupDto createDto)
    {
        var existingMuscleGroup = await _muscleGroupRepository.GetByNameAsync(createDto.Name);
        if (existingMuscleGroup != null)
        {
            throw new InvalidOperationException($"Muscle group with name '{createDto.Name}' already exists.");
        }

        var muscleGroup = new MuscleGroup { Name = createDto.Name };
        var createdEntity = await _muscleGroupRepository.AddAsync(muscleGroup);
        await _dbContext.SaveChangesAsync();

        return new MuscleGroupDto { Id = createdEntity.Id, Name = createdEntity.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            return false;
        }

        await _muscleGroupRepository.DeleteAsync(muscleGroup);
        var changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<MuscleGroupDto>> GetAllAsync()
    {
        var muscleGroups = await _muscleGroupRepository.GetAllAsync();
        return muscleGroups.Select(mg => new MuscleGroupDto { Id = mg.Id, Name = mg.Name });
    }

    public async Task<MuscleGroupDto?> GetByIdAsync(int id)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            return null;
        }

        return new MuscleGroupDto { Id = muscleGroup.Id, Name = muscleGroup.Name };
    }

    public async Task<bool> UpdateAsync(int id, UpdateMuscleGroupDto updateDto)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            return false;
        }

        var duplicateNameMuscleGroup = await _muscleGroupRepository.GetByNameAsync(updateDto.Name);
        if (duplicateNameMuscleGroup != null && duplicateNameMuscleGroup.Id != id)
        {
            throw new InvalidOperationException($"Another muscle group with name '{updateDto.Name}' already exists.");
        }

        muscleGroup.Name = updateDto.Name;
        await _muscleGroupRepository.UpdateAsync(muscleGroup);
        var changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            return true;
        }

        return false;
    }
}