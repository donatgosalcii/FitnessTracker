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

        public ExerciseService(
            IExerciseRepository exerciseRepository,
            IMuscleGroupRepository muscleGroupRepository,
            IApplicationDbContext dbContext)
        
        {
            _exerciseRepository = exerciseRepository;
            _muscleGroupRepository = muscleGroupRepository;
            _dbContext = dbContext;
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto createDto)
        {
            var existingExercise = await _exerciseRepository.GetByNameAsync(createDto.Name);
            if (existingExercise != null)
            {
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
                        throw new KeyNotFoundException($"Muscle group with ID {mgId} not found.");
                    }
                }
            }

            var createdEntity = await _exerciseRepository.AddAsync(exercise);
            await _dbContext.SaveChangesAsync();

            return MapExerciseToDto(createdEntity);
        }

        public async Task<bool> DeleteExerciseAsync(int id)
        {
            var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id); 
            if (exercise == null)
            {
                return false;
            }

            await _exerciseRepository.DeleteAsync(exercise);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
        {
            var exercises = await _exerciseRepository.GetAllWithDetailsAsync();
            return exercises.Select(MapExerciseToDto);
        }

        public async Task<ExerciseDto?> GetExerciseByIdAsync(int id)
        {
            var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id); 
            if (exercise == null)
            {
                return null;
            }
            return MapExerciseToDto(exercise);
        }

       public async Task<ExerciseDto?> UpdateExerciseAsync(int id, UpdateExerciseDto updateDto)
{
    var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
    if (exercise == null)
    {
        return null;
    }

    var duplicateNameExercise = await _exerciseRepository.GetByNameAsync(updateDto.Name);
    if (duplicateNameExercise != null && duplicateNameExercise.Id != id)
    {
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
            return MapExerciseToDto(updatedExerciseAfterSave);
        }
        else
        {
            return null; 
        }
    }
    
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