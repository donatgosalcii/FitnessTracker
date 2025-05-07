using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly IMuscleGroupRepository _muscleGroupRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<MuscleGroupService> _logger;

    public MuscleGroupService(
        IMuscleGroupRepository muscleGroupRepository,
        IApplicationDbContext dbContext,
        ILogger<MuscleGroupService> logger)
    {
        _muscleGroupRepository = muscleGroupRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<MuscleGroupDto> CreateAsync(CreateMuscleGroupDto createDto)
    {
        var existingMuscleGroup = await _muscleGroupRepository.GetByNameAsync(createDto.Name);
        if (existingMuscleGroup != null)
        {
            _logger.LogWarning("Muscle group with name '{Name}' already exists.", createDto.Name);
            throw new InvalidOperationException($"Muscle group with name '{createDto.Name}' already exists.");
        }

        var muscleGroup = new MuscleGroup { Name = createDto.Name };
        var createdEntity = await _muscleGroupRepository.AddAsync(muscleGroup);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Muscle group created: Id {Id}, Name {Name}", createdEntity.Id, createdEntity.Name);
        return new MuscleGroupDto { Id = createdEntity.Id, Name = createdEntity.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            _logger.LogWarning("Muscle group with Id {Id} not found for deletion.", id);
            return false;
        }

        await _muscleGroupRepository.DeleteAsync(muscleGroup);
        var changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            _logger.LogInformation("Muscle group deleted: Id {Id}", id);
            return true;
        }

        _logger.LogWarning("Muscle group with Id {Id} was not deleted, or no changes were persisted.", id);
        return false;
    }

    public async Task<IEnumerable<MuscleGroupDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all muscle groups.");
        var muscleGroups = await _muscleGroupRepository.GetAllAsync();
        return muscleGroups.Select(mg => new MuscleGroupDto { Id = mg.Id, Name = mg.Name });
    }

    public async Task<MuscleGroupDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving muscle group by Id: {Id}", id);
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            _logger.LogWarning("Muscle group with Id {Id} not found.", id);
            return null;
        }

        return new MuscleGroupDto { Id = muscleGroup.Id, Name = muscleGroup.Name };
    }

    public async Task<bool> UpdateAsync(int id, UpdateMuscleGroupDto updateDto)
    {
        var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
        if (muscleGroup == null)
        {
            _logger.LogWarning("Muscle group with Id {Id} not found for update.", id);
            return false;
        }

        var duplicateNameMuscleGroup = await _muscleGroupRepository.GetByNameAsync(updateDto.Name);
        if (duplicateNameMuscleGroup != null && duplicateNameMuscleGroup.Id != id)
        {
            _logger.LogWarning("Cannot update muscle group Id {Id}: Name '{Name}' already exists for Id {DuplicateId}",
                id, updateDto.Name, duplicateNameMuscleGroup.Id);
            throw new InvalidOperationException($"Another muscle group with name '{updateDto.Name}' already exists.");
        }

        muscleGroup.Name = updateDto.Name;
        await _muscleGroupRepository.UpdateAsync(muscleGroup);
        var changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            _logger.LogInformation("Muscle group updated: Id {Id}, New Name {Name}", id, updateDto.Name);
            return true;
        }

        _logger.LogWarning(
            "Muscle group with Id {Id} was not updated, or no changes were persisted (name might be the same).", id);
        return false;
    }
}