using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.User;
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
    public class ProfilesController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProfilesController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile() { var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); if (string.IsNullOrEmpty(currentUserId)) { return Unauthorized(new { Message = "User not authenticated." }); } var result = await _userService.GetUserProfileByIdAsync(currentUserId, currentUserId); if (result.IsSuccess) { return Ok(result.Value); } var errorMessage = result.Error?.Message ?? "An error occurred."; return result.ErrorType switch { ErrorType.NotFound => NotFound(new { Message = errorMessage }), ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error." })}; }
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(string userId) { var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); var result = await _userService.GetUserProfileByIdAsync(userId, requestingUserId); if (result.IsSuccess) { return Ok(result.Value); } var errorMessage = result.Error?.Message ?? "An error occurred."; return result.ErrorType switch { ErrorType.NotFound => NotFound(new { Message = errorMessage }), ErrorType.Unauthorized => Unauthorized(new { Message = "Not authorized." }), ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error." })}; }


        [HttpPut("me")]
        public async Task<ActionResult<UserProfileDto>> UpdateMyProfile(
            [FromForm] UpdateUserProfileDto updateDto, 
            IFormFile? profilePictureFile)        
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { Message = "User not authenticated." });
            }

            var result = await _userService.UpdateMyProfileAsync(currentUserId, updateDto, profilePictureFile);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            var errorMessage = result.Error?.Message ?? "An error occurred while updating your profile.";
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { Message = errorMessage }),
                ErrorType.Validation => BadRequest(new { Message = errorMessage, Code = result.Error?.Code }),
                ErrorType.Conflict => Conflict(new { Message = errorMessage, Code = result.Error?.Code }), 
                ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected server error occurred." }),
            };
        }
    }
}