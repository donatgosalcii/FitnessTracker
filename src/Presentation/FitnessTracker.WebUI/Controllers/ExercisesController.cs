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
            var exercises = await _exerciseService.GetAllExercisesAsync();
            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseDto>> GetExerciseById(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);

            if (exercise == null)
            {
                return NotFound(new { Message = $"Exercise with ID {id} not found." });
            }
            return Ok(exercise);
        }

        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ExerciseDto>> CreateExercise(CreateExerciseDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdExercise = await _exerciseService.CreateExerciseAsync(createDto);
                return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.Id }, createdExercise);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (System.Exception) 
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ExerciseDto>> UpdateExercise(int id, UpdateExerciseDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedExercise = await _exerciseService.UpdateExerciseAsync(id, updateDto);

                if (updatedExercise == null)
                {
                    return NotFound(new { Message = $"Exercise with ID {id} not found." });
                }
                return Ok(updatedExercise);
            }
            catch (KeyNotFoundException ex) 
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { Message = ex.Message });
            }
             catch (System.Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            try
            {
                var success = await _exerciseService.DeleteExerciseAsync(id);
                if (success)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(new { Message = $"Exercise with ID {id} not found." });
                }
            }
            catch (System.Exception) 
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}