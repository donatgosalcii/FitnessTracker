using FitnessTracker.Application.Common; 
using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ExercisesController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExercisesController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService; 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetAllExercises()
        {
            var result = await _exerciseService.GetAllExercisesAsync();
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
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An undefined error occurred on the server." }) // Default catch-all
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseDto>> GetExerciseById(int id)
        {
            var result = await _exerciseService.GetExerciseByIdAsync(id);
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
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An undefined error occurred on the server." })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ExerciseDto>> CreateExercise(CreateExerciseDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _exerciseService.CreateExerciseAsync(createDto);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetExerciseById), new { id = result.Value.Id }, result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(new { Message = errorMessage }), 
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),   
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.NotFound => NotFound(new { Message = errorMessage }), 
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An undefined error occurred on the server." })
            };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ExerciseDto>> UpdateExercise(int id, UpdateExerciseDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _exerciseService.UpdateExerciseAsync(id, updateDto);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An undefined error occurred on the server." })
            };
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var result = await _exerciseService.DeleteExerciseAsync(id);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            
            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }), 
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An undefined error occurred on the server." })
            };
        }
    }
}