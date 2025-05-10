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

        public WorkoutsController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        private string? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutSummaryDto>>> GetMyWorkouts()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token or token invalid." });
            }

            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);
            return Ok(workouts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkoutDetailDto>> GetWorkoutDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token or token invalid." });
            }

            var workout = await _workoutService.GetWorkoutDetailsAsync(id, userId);

            if (workout == null)
            {
                return NotFound(new { Message = $"Workout with ID {id} not found or access denied." });
            }

            return Ok(workout);
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutDetailDto>> LogWorkout([FromBody] LogWorkoutDto logDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token or token invalid." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdWorkout = await _workoutService.LogWorkoutAsync(logDto, userId);
                return CreatedAtAction(nameof(GetWorkoutDetails), new { id = createdWorkout.Id }, createdWorkout);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { Message = ex.Message }); 
            }
            catch (System.Exception) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while logging workout." });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWorkout(int id, [FromBody] UpdateWorkoutDto updateDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token or token invalid." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _workoutService.UpdateWorkoutAsync(id, userId, updateDto);

                if (success)
                {
                    var updatedWorkoutDetails = await _workoutService.GetWorkoutDetailsAsync(id, userId);
                    if (updatedWorkoutDetails == null) {
                        return NotFound(new { Message = $"Workout with ID {id} was updated but could not be retrieved."});
                    }
                    return Ok(updatedWorkoutDetails);
                }
                else
                {
                    var workoutExists = await _workoutService.GetWorkoutDetailsAsync(id, userId);
                    if (workoutExists == null)
                    {
                         return NotFound(new { Message = $"Workout with ID {id} not found or access denied." });
                    }
                    return BadRequest(new { Message = "Failed to update workout. Please check data and try again."});
                }
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex) 
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while updating workout."});
            }
        }

        [HttpDelete("{id:int}")] 
        public async Task<IActionResult> DeleteWorkout(int id)
        {
             var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token or token invalid."});
            }

            try
            {
                var success = await _workoutService.DeleteWorkoutAsync(id, userId);
                if (success)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(new { Message = $"Workout with ID {id} not found or access denied." });
                }
            }
            catch (System.Exception) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while deleting workout."});
            }
        }
    }
}