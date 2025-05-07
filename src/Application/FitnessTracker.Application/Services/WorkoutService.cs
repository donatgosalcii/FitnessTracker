using FitnessTracker.Application.DTOs.Workout;
using FitnessTracker.Application.DTOs.WorkoutSet;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<WorkoutService> _logger;

        public WorkoutService(
            IWorkoutRepository workoutRepository,
            IExerciseRepository exerciseRepository,
            IApplicationDbContext dbContext,
            ILogger<WorkoutService> logger)
        {
            _workoutRepository = workoutRepository;
            _exerciseRepository = exerciseRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkoutDetailDto> LogWorkoutAsync(LogWorkoutDto logDto, string userId)
        {
            _logger.LogInformation("Attempting to log workout for user {UserId} performed on {DatePerformed}", userId, logDto.DatePerformed);

            if (logDto.Sets == null || !logDto.Sets.Any())
            {
                _logger.LogWarning("LogWorkoutAsync failed for user {UserId}: No sets provided.", userId);
                throw new ArgumentException("Workout must contain at least one set.");
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
                    _logger.LogError("LogWorkoutAsync failed for user {UserId}: Invalid ExerciseId {ExerciseId} provided in set.", userId, setDto.ExerciseId);
                    throw new KeyNotFoundException($"Exercise with ID {setDto.ExerciseId} not found.");
                }

                if (!setDto.HasPerformanceMetric())
                {
                     _logger.LogWarning("LogWorkoutAsync for user {UserId}: Set for exercise {ExerciseId} has no performance metrics (Reps, Weight, Duration, Distance).", userId, setDto.ExerciseId);
                }

                workout.Sets.Add(new WorkoutSet
                {
                    ExerciseId = setDto.ExerciseId,
                    Workout = workout,
                    SetNumber = setDto.SetNumber,
                    Reps = setDto.Reps,
                    Weight = setDto.Weight,
                    DurationSeconds = setDto.DurationSeconds,
                    Distance = setDto.Distance,
                    Notes = setDto.Notes ?? string.Empty
                });
            }

            var createdWorkout = await _workoutRepository.AddAsync(workout);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Workout logged successfully for user {UserId} with Workout ID {WorkoutId}", userId, createdWorkout.Id);

            var workoutDetails = await GetWorkoutDetailsAsync(createdWorkout.Id, userId);

            if (workoutDetails == null)
            {
                 _logger.LogError("Failed to retrieve workout details immediately after logging for Workout ID {WorkoutId} and User ID {UserId}", createdWorkout.Id, userId);
                 throw new InvalidOperationException("Failed to retrieve workout details after logging.");
            }

            return workoutDetails;
        }

        public async Task<bool> DeleteWorkoutAsync(int workoutId, string userId)
        {
            _logger.LogInformation("Attempting to delete workout {WorkoutId} for user {UserId}", workoutId, userId);

            var workout = await _workoutRepository.GetWorkoutByIdAsync(workoutId, userId);
            if (workout == null)
            {
                _logger.LogWarning("DeleteWorkoutAsync failed: Workout {WorkoutId} not found or not owned by user {UserId}", workoutId, userId);
                return false;
            }

            await _workoutRepository.DeleteAsync(workout);
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation("Workout {WorkoutId} deleted successfully for user {UserId}", workoutId, userId);
                return true;
            }
            
            _logger.LogWarning("Workout {WorkoutId} owned by user {UserId} was not deleted, or no changes were persisted.", workoutId, userId);
            return false;
        }

        public async Task<WorkoutDetailDto?> GetWorkoutDetailsAsync(int workoutId, string userId)
        {
            _logger.LogInformation("Retrieving details for workout {WorkoutId} for user {UserId}", workoutId, userId);

            var workout = await _workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId, userId);

            if (workout == null)
            {
                _logger.LogWarning("Workout details not found for workout {WorkoutId} owned by user {UserId}", workoutId, userId);
                return null;
            }

            return new WorkoutDetailDto
            {
                Id = workout.Id,
                DatePerformed = workout.DatePerformed,
                Notes = workout.Notes ?? string.Empty,
                Sets = workout.Sets
                              .OrderBy(s => s.SetNumber)
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
                              }).ToList()
            };
        }

        public async Task<IEnumerable<WorkoutSummaryDto>> GetUserWorkoutsAsync(string userId)
        {
            _logger.LogInformation("Retrieving workout summaries for user {UserId}", userId);
            var workouts = await _workoutRepository.GetWorkoutsForUserAsync(userId);

            return workouts.Select(w => new WorkoutSummaryDto
            {
                Id = w.Id,
                DatePerformed = w.DatePerformed,
                Notes = w.Notes ?? string.Empty
            }).ToList();
        }
    }
}