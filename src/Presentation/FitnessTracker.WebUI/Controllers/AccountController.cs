using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userService, ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        try
        {
            var result = await _userService.RegisterUserAsync(registrationDto);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Registration successful" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code ?? string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Unexpected error during registration for {Email}", registrationDto.Email);
             return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred during registration." });
        }
    }

    // Add Login / Logout endpoints here later...
}