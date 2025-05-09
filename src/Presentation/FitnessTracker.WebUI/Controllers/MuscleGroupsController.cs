using FitnessTracker.Application.DTOs.MuscleGroup;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MuscleGroupsController : ControllerBase
{
    private readonly IMuscleGroupService _muscleGroupService;
    private readonly ILogger<MuscleGroupsController> _logger;

    public MuscleGroupsController(IMuscleGroupService muscleGroupService, ILogger<MuscleGroupsController> logger)
    {
        _muscleGroupService = muscleGroupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MuscleGroupDto>>> GetAllMuscleGroups()
    {
        _logger.LogInformation("Attempting to retrieve all muscle groups.");
        var muscleGroups = await _muscleGroupService.GetAllAsync();
        return Ok(muscleGroups);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MuscleGroupDto>> GetMuscleGroupById(int id)
    {
        _logger.LogInformation("Attempting to retrieve muscle group with ID: {Id}", id);
        var muscleGroup = await _muscleGroupService.GetByIdAsync(id);

        if (muscleGroup == null)
        {
            _logger.LogWarning("Muscle group with ID: {Id} not found.", id);
            return NotFound(new { Message = $"Muscle group with ID {id} not found." });
        }

        return Ok(muscleGroup);
    }

    [HttpPost]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<MuscleGroupDto>> CreateMuscleGroup(CreateMuscleGroupDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("Attempting to create muscle group with name: {Name}", createDto.Name);
        try
        {
            var createdMuscleGroup = await _muscleGroupService.CreateAsync(createDto);
            _logger.LogInformation("Muscle group created successfully with ID: {Id}",
                createdMuscleGroup.Id); // Added log
            return CreatedAtAction(nameof(GetMuscleGroupById), new { id = createdMuscleGroup.Id }, createdMuscleGroup);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create muscle group: {ErrorMessage}", ex.Message);
            return Conflict(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating muscle group with name: {Name}", createDto.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateMuscleGroup(int id, UpdateMuscleGroupDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _logger.LogInformation("Attempting to update muscle group with ID: {Id}", id);
        try
        {
            var success = await _muscleGroupService.UpdateAsync(id, updateDto);
            if (success)
            {
                _logger.LogInformation("Muscle group ID {Id} updated successfully.", id);
                return NoContent();
            }

            var exists = await _muscleGroupService.GetByIdAsync(id);
            if (exists == null)
            {
                _logger.LogWarning("Update failed: Muscle group with ID: {Id} not found.", id);
                return NotFound(new { Message = $"Muscle group with ID {id} not found." });
            }

            _logger.LogWarning("Update for muscle group with ID: {Id} resulted in no changes or other failure.", id);
            return BadRequest(new { Message = "Muscle group update failed or no changes were made." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update muscle group ID {Id}: {ErrorMessage}", id, ex.Message);
            return Conflict(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating muscle group ID: {Id}", id);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteMuscleGroup(int id)
    {
        _logger.LogInformation("Attempting to delete muscle group with ID: {Id}", id);
        try
        {
            var success = await _muscleGroupService.DeleteAsync(id);
            if (success)
            {
                _logger.LogInformation("Muscle group ID {Id} deleted successfully.", id);
                return NoContent();
            }

            _logger.LogWarning("Delete failed: Muscle group with ID: {Id} not found or not deleted.", id);
            return NotFound(new { Message = $"Muscle group with ID {id} not found or could not be deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting muscle group ID: {Id}", id);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}