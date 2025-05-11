using FitnessTracker.Application.Common; 
using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Services
{
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

        public async Task<Result<MuscleGroupDto>> CreateAsync(CreateMuscleGroupDto createDto)
        {
            try
            {
                var existingMuscleGroup = await _muscleGroupRepository.GetByNameAsync(createDto.Name);
                if (existingMuscleGroup != null)
                {
                    return Result<MuscleGroupDto>.Conflict($"Muscle group with name '{createDto.Name}' already exists.");
                }

                var muscleGroup = new MuscleGroup { Name = createDto.Name };
                var createdEntity = await _muscleGroupRepository.AddAsync(muscleGroup);
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                return Result<MuscleGroupDto>.Success(new MuscleGroupDto { Id = createdEntity.Id, Name = createdEntity.Name });
            }
            catch (System.Exception ex) 
            {
                return Result<MuscleGroupDto>.Unexpected($"An error occurred while creating the muscle group: {ex.Message}");
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
                if (muscleGroup == null)
                {
                    return Result.NotFound($"Muscle group with ID {id} not found.");
                }

                await _muscleGroupRepository.DeleteAsync(muscleGroup); 
                var changes = await _dbContext.SaveChangesAsync(CancellationToken.None);

                if (changes > 0)
                {
                    return Result.Success();
                }
                
                return Result.Failure("Muscle group was found but could not be deleted from the database, or no changes were made.", ErrorType.Failure);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) 
            {
                 if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FOREIGN KEY constraint")) {
                    return Result.Conflict($"Muscle group with ID {id} cannot be deleted as it is referenced by other entities (e.g., exercises).");
                }
                return Result.Unexpected($"A database error occurred while deleting the muscle group: {dbEx.Message}");
            }
            catch (System.Exception ex)
            {
                return Result.Unexpected($"An error occurred while deleting the muscle group: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<MuscleGroupDto>>> GetAllAsync()
        {
            try
            {
                var muscleGroups = await _muscleGroupRepository.GetAllAsync();
                return Result<IEnumerable<MuscleGroupDto>>.Success(
                    muscleGroups.Select(mg => new MuscleGroupDto { Id = mg.Id, Name = mg.Name })
                );
            }
            catch (System.Exception ex)
            {
                return Result<IEnumerable<MuscleGroupDto>>.Unexpected($"An error occurred while retrieving all muscle groups: {ex.Message}");
            }
        }

        public async Task<Result<MuscleGroupDto>> GetByIdAsync(int id)
        {
            try
            {
                var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
                if (muscleGroup == null)
                {
                    return Result<MuscleGroupDto>.NotFound($"Muscle group with ID {id} not found.");
                }
                return Result<MuscleGroupDto>.Success(new MuscleGroupDto { Id = muscleGroup.Id, Name = muscleGroup.Name });
            }
            catch (System.Exception ex)
            {
                return Result<MuscleGroupDto>.Unexpected($"An error occurred while retrieving muscle group ID {id}: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(int id, UpdateMuscleGroupDto updateDto)
        {
            try
            {
                var muscleGroup = await _muscleGroupRepository.GetByIdAsync(id);
                if (muscleGroup == null)
                {
                    return Result.NotFound($"Muscle group with ID {id} not found.");
                }

                if (!muscleGroup.Name.Equals(updateDto.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    var duplicateNameMuscleGroup = await _muscleGroupRepository.GetByNameAsync(updateDto.Name);
                    if (duplicateNameMuscleGroup != null && duplicateNameMuscleGroup.Id != id)
                    {
                        return Result.Conflict($"Another muscle group with name '{updateDto.Name}' already exists.");
                    }
                }
                
                bool hasChanges = false;
                if (muscleGroup.Name != updateDto.Name)
                {
                    muscleGroup.Name = updateDto.Name;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    return Result.Success(); 
                }

                await _muscleGroupRepository.UpdateAsync(muscleGroup);
                var changes = await _dbContext.SaveChangesAsync(CancellationToken.None);

                if (changes > 0)
                {
                    return Result.Success();
                }
                return Result.Failure("Muscle group was found but the update could not be saved, or no changes were made to save.", ErrorType.Failure);
            }
            catch (System.Exception ex)
            {
                return Result.Unexpected($"An error occurred while updating muscle group ID {id}: {ex.Message}");
            }
        }
    }
}