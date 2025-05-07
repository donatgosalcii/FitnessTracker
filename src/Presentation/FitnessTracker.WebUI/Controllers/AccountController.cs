using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers
{
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
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUserAsync(registrationDto);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} registered successfully.", registrationDto.Email);
                return Ok(new { Message = "User registered successfully." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            _logger.LogWarning("User registration failed for {Email}. Errors: {Errors}", registrationDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login attempt received for {Email}", loginDto.Email);
            var loginResponse = await _userService.LoginUserAsync(loginDto);

            if (loginResponse.IsSuccess && loginResponse.Token != null)
            {
                _logger.LogInformation("Login successful for {Email}, token issued.", loginDto.Email);
                return Ok(loginResponse);
            }

            _logger.LogWarning("Login failed for {Email}. Reason: {Reason}", loginDto.Email, loginResponse.Message);
            return Unauthorized(new { Message = loginResponse.Message });
        }
    }
}//will be refactored in the future