using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult GetPublicData()
        {
            _logger.LogInformation("Accessed /api/values/public");
            return Ok(new { Message = "This is public data, anyone can see this!" });
        }

        [HttpGet("secure")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
        public IActionResult GetSecureData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity?.Name;

            _logger.LogInformation("User {UserId} ({UserEmail}) accessed secure data (/api/values/secure).", userId, userEmail);

            return Ok(new
            {
                Message = $"Hello {userName}! This is SECURE data. Only authenticated users can see this.",
                YourUserId = userId,
                YourEmail = userEmail,
                AllYourClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
        public IActionResult GetAdminData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} (Admin) accessed admin-only data (/api/values/admin).", userId);

            return Ok(new { Message = "Welcome Admin! This data is for administrators ONLY." });
        }
    }
}