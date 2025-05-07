using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.DTOs.MuscleGroup; 
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMuscleGroupRepository _muscleGroupRepository;
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ExerciseService> _logger; 

        public ExerciseService(
            IExerciseRepository exerciseRepository,
            IMuscleGroupRepository muscleGroupRepository,
            IApplicationDbContext dbContext,
            ILogger<ExerciseService> logger)
        {
            _exerciseRepository = exerciseRepository;
            _muscleGroupRepository = muscleGroupRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto createDto)
        {
            var existingExercise = await _exerciseRepository.GetByNameAsync(createDto.Name);
            if (existingExercise != null)
            {
                _logger.LogWarning("Exercise with name '{Name}' already exists.", createDto.Name);
                throw new InvalidOperationException($"An exercise with the name '{createDto.Name}' already exists.");
            }

            var exercise = new Exercise
            {
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                MuscleGroups = new List<MuscleGroup>() 
            };

            if (createDto.MuscleGroupIds != null && createDto.MuscleGroupIds.Any())
            {
                foreach (var mgId in createDto.MuscleGroupIds.Distinct()) 
                {
                    var muscleGroup = await _muscleGroupRepository.GetByIdAsync(mgId);
                    if (muscleGroup != null)
                    {
                        exercise.MuscleGroups.Add(muscleGroup);
                    }
                    else
                    {
                        _logger.LogWarning("Muscle group with ID {Id} not found while creating exercise '{ExerciseName}'.", mgId, createDto.Name);
                        throw new KeyNotFoundException($"Muscle group with ID {mgId} not found.");
                    }
                }
            }

            var createdEntity = await _exerciseRepository.AddAsync(exercise);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Exercise created: Id {Id}, Name {Name}", createdEntity.Id, createdEntity.Name);
            return MapExerciseToDto(createdEntity);
        }

        public async Task<bool> DeleteExerciseAsync(int id)
        {
            var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id); 
            if (exercise == null)
            {
                _logger.LogWarning("Exercise with ID {Id} not found for deletion.", id);
                return false;
            }

            await _exerciseRepository.DeleteAsync(exercise);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Exercise deleted: Id {Id}", id);
                return true;
            }
            _logger.LogWarning("Exercise with ID {Id} was not deleted, or no changes were persisted.", id);
            return false;
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
        {
            _logger.LogInformation("Retrieving all exercises with details.");
            var exercises = await _exerciseRepository.GetAllWithDetailsAsync();
            return exercises.Select(MapExerciseToDto);
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving exercise by Id: {Id} with details.", id);
            var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id); 
            if (exercise == null)
            {
                _logger.LogWarning("Exercise with ID {Id} not found.", id);
                return null;
            }
            return MapExerciseToDto(exercise);
        }

       public async Task<ExerciseDto?> UpdateExerciseAsync(int id, UpdateExerciseDto updateDto)
{
    var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
    if (exercise == null)
    {
        _logger.LogWarning("Exercise with ID {Id} not found for update.", id);
        return null;
    }

    var duplicateNameExercise = await _exerciseRepository.GetByNameAsync(updateDto.Name);
    if (duplicateNameExercise != null && duplicateNameExercise.Id != id)
    {
        _logger.LogWarning("Cannot update exercise Id {ExerciseId}: Name '{Name}' already exists for exercise Id {DuplicateId}", id, updateDto.Name, duplicateNameExercise.Id);
        throw new InvalidOperationException($"Another exercise with the name '{updateDto.Name}' already exists.");
    }

    exercise.Name = updateDto.Name;
    exercise.Description = updateDto.Description ?? string.Empty;

    exercise.MuscleGroups.Clear();
    if (updateDto.MuscleGroupIds != null && updateDto.MuscleGroupIds.Any())
    {
        foreach (var mgId in updateDto.MuscleGroupIds.Distinct())
        {
            var muscleGroup = await _muscleGroupRepository.GetByIdAsync(mgId);
            if (muscleGroup != null)
            {
                exercise.MuscleGroups.Add(muscleGroup);
            }
            else
            {
                _logger.LogWarning("Muscle group with ID {Id} not found while updating exercise '{ExerciseName}'.", mgId, updateDto.Name);
                throw new KeyNotFoundException($"Muscle group with ID {mgId} not found.");
            }
        }
    }

    await _exerciseRepository.UpdateAsync(exercise);
    var changes = await _dbContext.SaveChangesAsync();

    if (changes >= 0) 
    {
        Exercise? updatedExerciseAfterSave = await _exerciseRepository.GetByIdWithDetailsAsync(id); 
        if (updatedExerciseAfterSave != null)
        {
            _logger.LogInformation("Exercise updated: Id {Id}", id);
            return MapExerciseToDto(updatedExerciseAfterSave);
        }
        else
        {
            _logger.LogError("Exercise with ID {Id} could not be refetched after update attempt.", id);
            return null; 
        }
    }
    
    _logger.LogWarning("Exercise with ID {Id} was not updated properly, or an issue occurred during save.", id);
    return null;
    }

        private ExerciseDto MapExerciseToDto(Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description ?? string.Empty,
                MuscleGroups = exercise.MuscleGroups? 
                    .Where(mg => mg != null) 
                    .Select(mg => new MuscleGroupDto {
                        Id = mg.Id, 
                        Name = mg.Name! 
                    })
                    .ToList() ?? new List<MuscleGroupDto>()
            };
        }
    }
}