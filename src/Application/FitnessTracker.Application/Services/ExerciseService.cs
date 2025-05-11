using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;

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

        public async Task<Result<ExerciseDto>> CreateExerciseAsync(CreateExerciseDto createDto)
        {
            try
            {
                var existingExercise = await _exerciseRepository.GetByNameAsync(createDto.Name);
                if (existingExercise != null)
                {
                    return Result<ExerciseDto>.Conflict($"An exercise with the name '{createDto.Name}' already exists.");
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
                            return Result<ExerciseDto>.ValidationFailed($"Muscle group with ID {mgId} not found. Exercise cannot be created.");
                        }
                    }
                }

                var createdEntity = await _exerciseRepository.AddAsync(exercise);
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                return Result<ExerciseDto>.Success(MapExerciseToDto(createdEntity));
            }
            catch (System.Exception ex)
            {
                return Result<ExerciseDto>.Unexpected($"An error occurred while creating the exercise: {ex.Message}");
            }
        }

        public async Task<Result> DeleteExerciseAsync(int id)
        {
            try
            {
                var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
                if (exercise == null)
                {
                    return Result.NotFound($"Exercise with ID {id} not found.");
                }

                await _exerciseRepository.DeleteAsync(exercise);
                var changes = await _dbContext.SaveChangesAsync(CancellationToken.None);

                if (changes > 0)
                {
                    return Result.Success();
                }
                return Result.Failure("Exercise was found but could not be deleted, or no changes were made.", ErrorType.Failure);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                 if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FOREIGN KEY constraint")) {
                    return Result.Conflict($"Exercise with ID {id} cannot be deleted as it is referenced by other entities (e.g., workout sets).");
                }
                return Result.Unexpected($"A database error occurred while deleting the exercise: {dbEx.Message}");
            }
            catch (System.Exception ex)
            {
                return Result.Unexpected($"An error occurred while deleting exercise with ID {id}: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<ExerciseDto>>> GetAllExercisesAsync()
        {
            try
            {
                var exercises = await _exerciseRepository.GetAllWithDetailsAsync();
                return Result<IEnumerable<ExerciseDto>>.Success(exercises.Select(MapExerciseToDto));
            }
            catch (System.Exception ex)
            {
                return Result<IEnumerable<ExerciseDto>>.Unexpected($"An error occurred while retrieving all exercises: {ex.Message}");
            }
        }

        public async Task<Result<ExerciseDto>> GetExerciseByIdAsync(int id)
        {
            try
            {
                var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
                if (exercise == null)
                {
                    return Result<ExerciseDto>.NotFound($"Exercise with ID {id} not found.");
                }
                return Result<ExerciseDto>.Success(MapExerciseToDto(exercise));
            }
            catch (System.Exception ex)
            {
                return Result<ExerciseDto>.Unexpected($"An error occurred while retrieving exercise ID {id}: {ex.Message}");
            }
        }

        public async Task<Result<ExerciseDto>> UpdateExerciseAsync(int id, UpdateExerciseDto updateDto)
        {
            try
            {
                var exercise = await _exerciseRepository.GetByIdWithDetailsAsync(id);
                if (exercise == null)
                {
                    return Result<ExerciseDto>.NotFound($"Exercise with ID {id} not found.");
                }

                if (!exercise.Name.Equals(updateDto.Name, System.StringComparison.OrdinalIgnoreCase))
                {
                    var duplicateNameExercise = await _exerciseRepository.GetByNameAsync(updateDto.Name);
                    if (duplicateNameExercise != null && duplicateNameExercise.Id != id)
                    {
                        return Result<ExerciseDto>.Conflict($"Another exercise with the name '{updateDto.Name}' already exists.");
                    }
                }

                exercise.Name = updateDto.Name;
                exercise.Description = updateDto.Description ?? string.Empty;

                var currentMgIds = exercise.MuscleGroups.Select(mg => mg.Id).ToList();
                var desiredMgIds = updateDto.MuscleGroupIds?.Distinct().ToList() ?? new List<int>();

                var mgIdsToRemove = currentMgIds.Except(desiredMgIds).ToList();
                var mgIdsToAdd = desiredMgIds.Except(currentMgIds).ToList();

                foreach (var mgIdToRemove in mgIdsToRemove)
                {
                    var mgToRemove = exercise.MuscleGroups.FirstOrDefault(mg => mg.Id == mgIdToRemove);
                    if (mgToRemove != null)
                    {
                        exercise.MuscleGroups.Remove(mgToRemove);
                    }
                }

                foreach (var mgIdToAdd in mgIdsToAdd)
                {
                    var muscleGroup = await _muscleGroupRepository.GetByIdAsync(mgIdToAdd);
                    if (muscleGroup != null)
                    {
                        exercise.MuscleGroups.Add(muscleGroup);
                    }
                    else
                    {
                        return Result<ExerciseDto>.ValidationFailed($"Muscle group with ID {mgIdToAdd} not found. Exercise update failed.");
                    }
                }

                await _exerciseRepository.UpdateAsync(exercise); 
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                var updatedExerciseFromDb = await _exerciseRepository.GetByIdWithDetailsAsync(id);
                if (updatedExerciseFromDb == null)
                {
                    return Result<ExerciseDto>.Unexpected("Failed to retrieve the exercise after update.");
                }
                return Result<ExerciseDto>.Success(MapExerciseToDto(updatedExerciseFromDb));
            }
            catch (System.Exception ex)
            {
                return Result<ExerciseDto>.Unexpected($"An error occurred while updating exercise ID {id}: {ex.Message}");
            }
        }

        private ExerciseDto MapExerciseToDto(Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description ?? string.Empty,
                MuscleGroups = exercise.MuscleGroups
                    ?.Where(mg => mg != null) 
                    .Select(mg => new MuscleGroupDto {
                        Id = mg.Id, 
                        Name = mg.Name ?? "Unnamed Muscle Group" 
                    })
                    .ToList() ?? new List<MuscleGroupDto>()
            };
        }
    }
}