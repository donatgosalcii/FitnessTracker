using FitnessTracker.Application.Common; 
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
            var result = await _muscleGroupService.GetAllAsync();
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
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MuscleGroupDto>> GetMuscleGroupById(int id)
        {
            var result = await _muscleGroupService.GetByIdAsync(id);
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
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<MuscleGroupDto>> CreateMuscleGroup(CreateMuscleGroupDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _muscleGroupService.CreateAsync(createDto);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetMuscleGroupById), new { id = result.Value.Id }, result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
            };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateMuscleGroup(int id, UpdateMuscleGroupDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _muscleGroupService.UpdateAsync(id, updateDto);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
            };
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteMuscleGroup(int id)
        {
            var result = await _muscleGroupService.DeleteAsync(id);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            
            var errorMessage = result.Error?.Message ?? "An unknown error occurred.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage }),
                ErrorType.Unauthorized => Unauthorized(new { Message = errorMessage }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage }),
                ErrorType.Failure => BadRequest(new { Message = errorMessage }),
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred on the server." }),
            };
        }
    }
}