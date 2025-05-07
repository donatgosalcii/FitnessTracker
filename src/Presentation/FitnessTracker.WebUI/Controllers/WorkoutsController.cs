using FitnessTracker.Application.DTOs.Workout;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("Found User ID using ClaimTypes.NameIdentifier.");
                return userId;
            }

            _logger.LogWarning("Could not find ClaimTypes.NameIdentifier, trying JwtRegisteredClaimNames.Sub.");
            userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!string.IsNullOrEmpty(userId))
            {
                 _logger.LogDebug("Found User ID using JwtRegisteredClaimNames.Sub.");
                return userId;
            }

            _logger.LogWarning("Could not find JwtRegisteredClaimNames.Sub, trying custom 'uid'.");
            userId = User.FindFirstValue("uid");
             if (!string.IsNullOrEmpty(userId))
            {
                 _logger.LogDebug("Found User ID using custom 'uid' claim.");
                return userId;
            }

            _logger.LogError("Could not find any expected user ID claim ('{NameIdentifier}', '{Sub}', 'uid') in the ClaimsPrincipal.", ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);
            return null;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutSummaryDto>>> GetMyWorkouts()
        {
            _logger.LogInformation("------ GetMyWorkouts START ------");
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                _logger.LogInformation("User is Authenticated. Claims found ({Count}):", User.Claims.Count());
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("  Claim Type: {ClaimType}, Value: {ClaimValue}", claim.Type, claim.Value);
                }
            }
            else
            {
                _logger.LogWarning("User is NOT Authenticated or Identity is null.");
            }
             _logger.LogInformation("------ End Claims Dump ------");


            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
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
                return Unauthorized("User ID not found in token.");
            }

            _logger.LogInformation("Retrieving details for workout {WorkoutId} for user {UserId}", id, userId);
            var workout = await _workoutService.GetWorkoutDetailsAsync(id, userId);

            if (workout == null)
            {
                _logger.LogWarning("Workout {WorkoutId} not found or not owned by user {UserId}.", id, userId);
                return NotFound(new { Message = $"Workout with ID {id} not found." });
            }

            return Ok(workout);
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutDetailDto>> LogWorkout(LogWorkoutDto logDto)
        {

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to log workout for user {UserId}", userId);
            try
            {
                var createdWorkout = await _workoutService.LogWorkoutAsync(logDto, userId);
                return CreatedAtAction(nameof(GetWorkoutDetails), new { id = createdWorkout.Id }, createdWorkout);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to log workout for user {UserId}: {ErrorMessage}", userId, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Failed to log workout for user {UserId}: {ErrorMessage}", userId, ex.Message);
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
                return Unauthorized("User ID not found in token.");
            }

            _logger.LogInformation("Attempting to delete workout {WorkoutId} for user {UserId}", id, userId);
            try
            {
                var success = await _workoutService.DeleteWorkoutAsync(id, userId);
                if (success)
                {
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning("Delete failed: Workout {WorkoutId} not found or not owned by user {UserId}.", id, userId);
                    return NotFound(new { Message = $"Workout with ID {id} not found." });
                }
            }
            catch (System.Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred while deleting workout {WorkoutId} for user {UserId}", id, userId);
                return StatusCode(500, "An unexpected error occurred while deleting workout.");
            }
        }
    }
}