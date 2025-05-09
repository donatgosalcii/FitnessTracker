using FitnessTracker.Application.DTOs.Workout;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WorkoutsController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly ILogger<WorkoutsController> _logger;

        public WorkoutsController(IWorkoutService workoutService, ILogger<WorkoutsController> logger)
        {
            _workoutService = workoutService;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID (ClaimTypes.NameIdentifier) not found in token for user: {UserName}", User.Identity?.Name);
            }
            else
            {
                _logger.LogInformation("Retrieved User ID {UserId} from token.", userId);
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutSummaryDto>>> GetMyWorkouts()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("GetMyWorkouts called but User ID could not be determined from token.");
                return Unauthorized("User ID not found in token or token invalid.");
            }

            _logger.LogInformation("Retrieving workouts for user {UserId}", userId);
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);
            return Ok(workouts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkoutDetailDto>> GetWorkoutDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                 _logger.LogWarning("GetWorkoutDetails called but User ID could not be determined from token.");
                return Unauthorized("User ID not found in token or token invalid.");
            }

            _logger.LogInformation("Retrieving details for workout {WorkoutId} for user {UserId}", id, userId);
            var workout = await _workoutService.GetWorkoutDetailsAsync(id, userId);

            if (workout == null)
            {
                _logger.LogWarning("Workout {WorkoutId} not found or not owned by user {UserId}.", id, userId);
                return NotFound(new { Message = $"Workout with ID {id} not found or access denied." });
            }

            return Ok(workout);
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutDetailDto>> LogWorkout(LogWorkoutDto logDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("LogWorkout called but User ID could not be determined from token.");
                return Unauthorized("User ID not found in token or token invalid.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to log workout for user {UserId}", userId);
            try
            {
                var createdWorkout = await _workoutService.LogWorkoutAsync(logDto, userId);
                 _logger.LogInformation("Workout logged successfully with ID {WorkoutId} for user {UserId}", createdWorkout.Id, userId);
                return CreatedAtAction(nameof(GetWorkoutDetails), new { id = createdWorkout.Id }, createdWorkout);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to log workout for user {UserId} due to invalid argument: {ErrorMessage}", userId, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Failed to log workout for user {UserId} due to KeyNotFound: {ErrorMessage}", userId, ex.Message);
                return BadRequest(new { Message = ex.Message }); 
            }
            catch (System.Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred while logging workout for user {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while logging workout.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
             var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("DeleteWorkout called but User ID could not be determined from token.");
                return Unauthorized("User ID not found in token or token invalid.");
            }

            _logger.LogInformation("Attempting to delete workout {WorkoutId} for user {UserId}", id, userId);
            try
            {
                var success = await _workoutService.DeleteWorkoutAsync(id, userId);
                if (success)
                {
                    _logger.LogInformation("Workout ID {WorkoutId} deleted successfully for user {UserId}.", id, userId);
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning("Delete failed: Workout {WorkoutId} not found or not owned by user {UserId}.", id, userId);
                    return NotFound(new { Message = $"Workout with ID {id} not found or access denied." });
                }
            }
            catch (System.Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred while deleting workout {WorkoutId} for user {UserId}", id, userId);
                return StatusCode(500, "An unexpected error occurred while deleting workout.");
            }
        }
         // TODO: Implement PUT /api/workouts/{id} for updating workout notes or date
    }
}