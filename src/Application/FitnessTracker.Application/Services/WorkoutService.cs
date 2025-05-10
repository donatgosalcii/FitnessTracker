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

        public async Task<WorkoutDetailDto> LogWorkoutAsync(LogWorkoutDto logDto, string userId)
        {

            if (logDto.Sets == null || !logDto.Sets.Any())
            {
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
                    throw new KeyNotFoundException($"Exercise with ID {setDto.ExerciseId} not found.");
                }

                if (!setDto.HasPerformanceMetric())
                {
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


            var workoutDetails = await GetWorkoutDetailsAsync(createdWorkout.Id, userId);
            if (workoutDetails == null)
            {
                 throw new InvalidOperationException("Failed to retrieve workout details after logging.");
            }
            return workoutDetails;
        }
        
        public async Task<bool> UpdateWorkoutAsync(int workoutId, string userId, UpdateWorkoutDto updateDto)
        {

           
            var workoutToUpdate = await _dbContext.Workouts
                                                 .Include(w => w.Sets) 
                                                 .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

            if (workoutToUpdate == null)
            {
                return false; 
            }

            workoutToUpdate.DatePerformed = updateDto.DatePerformed;
            workoutToUpdate.Notes = updateDto.Notes ?? string.Empty;

            if (workoutToUpdate.Sets != null) 
            {
              
                workoutToUpdate.Sets.Clear(); 
            } else {
                workoutToUpdate.Sets = new List<WorkoutSet>(); 
            }


            if (updateDto.Sets != null && updateDto.Sets.Any())
            {
                foreach (var setDto in updateDto.Sets)
                {
                    var exercise = await _exerciseRepository.GetByIdAsync(setDto.ExerciseId);
                    if (exercise == null)
                    {
                        throw new KeyNotFoundException($"Exercise with ID {setDto.ExerciseId} not found. Cannot update workout.");
                    }

                    if (!setDto.HasPerformanceMetric())
                    {
                    }

                    workoutToUpdate.Sets.Add(new WorkoutSet
                    {
                        ExerciseId = setDto.ExerciseId,
                        Workout = workoutToUpdate,
                        SetNumber = setDto.SetNumber,
                        Reps = setDto.Reps,
                        Weight = setDto.Weight,
                        DurationSeconds = setDto.DurationSeconds,
                        Distance = setDto.Distance,
                        Notes = setDto.Notes ?? string.Empty
                      
                    });
                }
            }
            else
            {
            }

            
            var changes = await _dbContext.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteWorkoutAsync(int workoutId, string userId)
        {

            var workout = await _workoutRepository.GetWorkoutByIdAsync(workoutId, userId); 
            if (workout == null)
            {
                return false;
            }

            await _workoutRepository.DeleteAsync(workout); 
            var changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return true;
            }
            
            return false;
        }

        public async Task<WorkoutDetailDto?> GetWorkoutDetailsAsync(int workoutId, string userId)
        {

            var workout = await _workoutRepository.GetWorkoutByIdWithDetailsAsync(workoutId, userId);

            if (workout == null)
            {
                return null;
            }

            return new WorkoutDetailDto
            {
                Id = workout.Id,
                DatePerformed = workout.DatePerformed,
                Notes = workout.Notes ?? string.Empty,
                Sets = workout.Sets
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
                              }).ToList()
            };
        }

        public async Task<IEnumerable<WorkoutSummaryDto>> GetUserWorkoutsAsync(string userId)
        {
            var workouts = await _workoutRepository.GetWorkoutsForUserAsync(userId);

            return workouts.Select(w => new WorkoutSummaryDto
            {
                Id = w.Id,
                DatePerformed = w.DatePerformed,
                Notes = w.Notes ?? string.Empty
            }).OrderByDescending(w => w.DatePerformed).ToList(); 
        }
    }
}