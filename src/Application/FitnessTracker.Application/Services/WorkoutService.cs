using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Workout;
using FitnessTracker.Application.DTOs.WorkoutSet;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Application.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IApplicationDbContext _dbContext; 

        public WorkoutService(
            IWorkoutRepository workoutRepository,
            IExerciseRepository exerciseRepository,
            IApplicationDbContext dbContext)
        {
            _workoutRepository = workoutRepository;
            _exerciseRepository = exerciseRepository;
            _dbContext = dbContext;
        }

        public async Task<Result<WorkoutDetailDto>> LogWorkoutAsync(LogWorkoutDto logDto, string userId)
        {
            if (logDto.Sets == null || !logDto.Sets.Any())
            {
                return Result<WorkoutDetailDto>.ValidationFailed("Workout must contain at least one set.");
            }

            var workout = new Workout
            {
                UserId = userId,
                DatePerformed = logDto.DatePerformed,
                Notes = logDto.Notes ?? string.Empty,
                Sets = new List<WorkoutSet>()
            };

            foreach (var setDto in logDto.Sets)
            {
                var exercise = await _exerciseRepository.GetByIdAsync(setDto.ExerciseId); 
                if (exercise == null)
                {
                    return Result<WorkoutDetailDto>.ValidationFailed($"Exercise with ID {setDto.ExerciseId} not found. Cannot log workout.");
                }

                workout.Sets.Add(new WorkoutSet
                {
                    ExerciseId = setDto.ExerciseId,
                    SetNumber = setDto.SetNumber,
                    Reps = setDto.Reps,
                    Weight = setDto.Weight,
                    DurationSeconds = setDto.DurationSeconds,
                    Distance = setDto.Distance,
                    Notes = setDto.Notes ?? string.Empty
                });
            }

            try
            {
                var createdWorkoutEntity = await _workoutRepository.AddAsync(workout);
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                return await GetWorkoutDetailsAsync(createdWorkoutEntity.Id, userId);
            }
            catch (System.Exception ex)
            {
                return Result<WorkoutDetailDto>.Unexpected($"An error occurred while logging the workout: {ex.Message}");
            }
        }
        
        public async Task<Result<WorkoutDetailDto>> UpdateWorkoutAsync(int workoutId, string userId, UpdateWorkoutDto updateDto)
        {
            try
            {
                var workoutToUpdate = await _dbContext.Workouts
                                                     .Include(w => w.Sets) 
                                                     .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

                if (workoutToUpdate == null)
                {
                    return Result<WorkoutDetailDto>.NotFound($"Workout with ID {workoutId} not found or access denied.");
                }

                workoutToUpdate.DatePerformed = updateDto.DatePerformed;
                workoutToUpdate.Notes = updateDto.Notes ?? string.Empty;

                workoutToUpdate.Sets.Clear(); 

                if (updateDto.Sets != null && updateDto.Sets.Any())
                {
                    foreach (var setDto in updateDto.Sets)
                    {
                        var exercise = await _exerciseRepository.GetByIdAsync(setDto.ExerciseId);
                        if (exercise == null)
                        {
                            return Result<WorkoutDetailDto>.ValidationFailed($"Exercise with ID {setDto.ExerciseId} not found. Cannot update workout.");
                        }

                        workoutToUpdate.Sets.Add(new WorkoutSet
                        {
                            ExerciseId = setDto.ExerciseId,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            DurationSeconds = setDto.DurationSeconds,
                            Distance = setDto.Distance,
                            Notes = setDto.Notes ?? string.Empty
                        });
                    }
                }
                
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                return await GetWorkoutDetailsAsync(workoutId, userId);
            }
            catch (System.Exception ex)
            {
                return Result<WorkoutDetailDto>.Unexpected($"An error occurred while updating workout ID {workoutId}: {ex.Message}");
            }
        }

        public async Task<Result> DeleteWorkoutAsync(int workoutId, string userId)
        {
            try
            {
                var workout = await _workoutRepository.GetWorkoutByIdAsync(workoutId, userId); 
                if (workout == null)
                {
                    return Result.NotFound($"Workout with ID {workoutId} not found or access denied.");
                }

                await _workoutRepository.DeleteAsync(workout); 
                var changes = await _dbContext.SaveChangesAsync(CancellationToken.None);

                if (changes > 0)
                {
                    return Result.Success();
                }
                return Result.Failure("Workout was found but could not be deleted from the database.", ErrorType.Failure);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                 if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FOREIGN KEY constraint")) {
                    return Result.Conflict($"Workout with ID {workoutId} cannot be deleted as it might be referenced by other data.");
                }
                return Result.Unexpected($"A database error occurred: {dbEx.Message}");
            }
            catch (System.Exception ex)
            {
                return Result.Unexpected($"An error occurred while deleting workout ID {workoutId}: {ex.Message}");
            }
        }

        public async Task<Result<WorkoutDetailDto>> GetWorkoutDetailsAsync(int workoutId, string userId)
        {
            try
            {
                var workout = await _workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId, userId);
                if (workout == null)
                {
                    return Result<WorkoutDetailDto>.NotFound($"Workout with ID {workoutId} not found or access denied.");
                }

                return Result<WorkoutDetailDto>.Success(new WorkoutDetailDto
                {
                    Id = workout.Id,
                    DatePerformed = workout.DatePerformed,
                    Notes = workout.Notes ?? string.Empty,
                    Sets = workout.Sets?
                                  .OrderBy(s => s.Exercise?.Name) 
                                  .ThenBy(s => s.SetNumber) 
                                  .Select(s => new WorkoutSetDetailDto
                                  {
                                      Id = s.Id,
                                      ExerciseId = s.ExerciseId,
                                      ExerciseName = s.Exercise?.Name ?? "Unknown Exercise",
                                      SetNumber = s.SetNumber,
                                      Reps = s.Reps,
                                      Weight = s.Weight,
                                      DurationSeconds = s.DurationSeconds,
                                      Distance = s.Distance,
                                      Notes = s.Notes ?? string.Empty 
                                  }).ToList() ?? new List<WorkoutSetDetailDto>()
                });
            }
            catch (System.Exception ex)
            {
                return Result<WorkoutDetailDto>.Unexpected($"An error occurred retrieving workout details for ID {workoutId}: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<WorkoutSummaryDto>>> GetUserWorkoutsAsync(string userId)
        {
            try
            {
                var workouts = await _workoutRepository.GetWorkoutsForUserAsync(userId);
                var dtos = workouts.Select(w => new WorkoutSummaryDto
                {
                    Id = w.Id,
                    DatePerformed = w.DatePerformed,
                    Notes = w.Notes ?? string.Empty
                }).OrderByDescending(w => w.DatePerformed).ToList();
                return Result<IEnumerable<WorkoutSummaryDto>>.Success(dtos);
            }
            catch (System.Exception ex)
            {
                return Result<IEnumerable<WorkoutSummaryDto>>.Unexpected($"An error occurred retrieving workouts for user: {ex.Message}");
            }
        }
    }
}