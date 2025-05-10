using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MuscleGroupsController : ControllerBase
    {
        private readonly IMuscleGroupService _muscleGroupService;

        public MuscleGroupsController(IMuscleGroupService muscleGroupService)
        {
            _muscleGroupService = muscleGroupService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MuscleGroupDto>>> GetAllMuscleGroups()
        {
            var muscleGroups = await _muscleGroupService.GetAllAsync();
            return Ok(muscleGroups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MuscleGroupDto>> GetMuscleGroupById(int id)
        {
            var muscleGroup = await _muscleGroupService.GetByIdAsync(id);

            if (muscleGroup == null)
            {
                return NotFound(new { Message = $"Muscle group with ID {id} not found." });
            }

            return Ok(muscleGroup);
        }

        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<MuscleGroupDto>> CreateMuscleGroup(CreateMuscleGroupDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var createdMuscleGroup = await _muscleGroupService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetMuscleGroupById), new { id = createdMuscleGroup.Id }, createdMuscleGroup);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { Message = ex.Message }); 
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateMuscleGroup(int id, UpdateMuscleGroupDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _muscleGroupService.UpdateAsync(id, updateDto);
                if (success)
                {
                    return NoContent();
                }

                var exists = await _muscleGroupService.GetByIdAsync(id);
                if (exists == null)
                {
                    return NotFound(new { Message = $"Muscle group with ID {id} not found." });
                }
                return BadRequest(new { Message = "Muscle group update failed or no changes were detected." });
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception) 
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteMuscleGroup(int id)
        {
            try
            {
                var success = await _muscleGroupService.DeleteAsync(id);
                if (success)
                {
                    return NoContent();
                }
                return NotFound(new { Message = $"Muscle group with ID {id} not found or could not be deleted." });
            }
            catch (Exception) 
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}