using FitnessTracker.Application.Common;
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
            return User.FindFirstValue(ClaimTypes.NameIdentifier); 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutSummaryDto>>> GetMyWorkouts()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            var result = await _workoutService.GetUserWorkoutsAsync(userId);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            var errorMessage = result.Error?.Message ?? "An unexpected error occurred.";
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = errorMessage });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkoutDetailDto>> GetWorkoutDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            var result = await _workoutService.GetWorkoutDetailsAsync(id, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            
            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error occurred." }),
            };
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutDetailDto>> LogWorkout([FromBody] LogWorkoutDto logDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var result = await _workoutService.LogWorkoutAsync(logDto, userId);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetWorkoutDetails), new { id = result.Value.Id }, result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error occurred." }),
            };
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WorkoutDetailDto>> UpdateWorkout(int id, [FromBody] UpdateWorkoutDto updateDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _workoutService.UpdateWorkoutAsync(id, userId, updateDto);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new {Message = errorMessage}),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error occurred." }),
            };
        }

        [HttpDelete("{id:int}")] 
        public async Task<IActionResult> DeleteWorkout(int id)
        {
             var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID not found in token."});
            }

            var result = await _workoutService.DeleteWorkoutAsync(id, userId);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new {Message = errorMessage}),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error occurred." }),
            };
        }
    }
}