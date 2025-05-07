using FitnessTracker.Application.DTOs.Exercise;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    [Authorize] 
    public class ExercisesController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;
        private readonly ILogger<ExercisesController> _logger;

        public ExercisesController(IExerciseService exerciseService, ILogger<ExercisesController> logger)
        {
            _exerciseService = exerciseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetAllExercises()
        {
            _logger.LogInformation("Attempting to retrieve all exercises.");
            var exercises = await _exerciseService.GetAllExercisesAsync();
            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseDto>> GetExerciseById(int id)
        {
            _logger.LogInformation("Attempting to retrieve exercise with ID: {Id}", id);
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);

            if (exercise == null)
            {
                _logger.LogWarning("Exercise with ID: {Id} not found.", id);
                return NotFound(new { Message = $"Exercise with ID {id} not found." });
            }
            return Ok(exercise);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admins can create
        public async Task<ActionResult<ExerciseDto>> CreateExercise(CreateExerciseDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to create exercise with name: {Name}", createDto.Name);
            try
            {
                var createdExercise = await _exerciseService.CreateExerciseAsync(createDto);
                return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.Id }, createdExercise);
            }
            catch (KeyNotFoundException ex) 
            {
                _logger.LogWarning("Failed to create exercise: {ErrorMessage}", ex.Message);
                return BadRequest(new { Message = ex.Message }); 
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning("Failed to create exercise: {ErrorMessage}", ex.Message);
                return Conflict(new { Message = ex.Message }); 
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating exercise with name: {Name}", createDto.Name);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExerciseDto>> UpdateExercise(int id, UpdateExerciseDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to update exercise with ID: {Id}", id);
            try
            {
                var updatedExercise = await _exerciseService.UpdateExerciseAsync(id, updateDto);

                if (updatedExercise == null)
                {
                     _logger.LogWarning("Update failed: Exercise with ID: {Id} not found.", id);
                    return NotFound(new { Message = $"Exercise with ID {id} not found." });
                }
                return Ok(updatedExercise);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Failed to update exercise ID {Id}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { Message = ex.Message }); 
            }
            catch (InvalidOperationException ex) 
            {
                 _logger.LogWarning("Failed to update exercise ID {Id}: {ErrorMessage}", id, ex.Message);
                return Conflict(new { Message = ex.Message });
            }
             catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating exercise ID: {Id}", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteExercise(int id)
        {
            _logger.LogInformation("Attempting to delete exercise with ID: {Id}", id);
            try
            {
                var success = await _exerciseService.DeleteExerciseAsync(id);
                if (success)
                {
                    return NoContent(); 
                }
                else
                {
                    _logger.LogWarning("Delete failed: Exercise with ID: {Id} not found.", id);
                    return NotFound(new { Message = $"Exercise with ID {id} not found." });
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting exercise ID: {Id}", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}