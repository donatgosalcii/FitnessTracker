using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FitnessTracker.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var serviceResult = await _userService.RegisterUserAndPrepareConfirmationAsync(registrationDto);

            if (serviceResult.IdentityResult.Succeeded && serviceResult.User != null && !string.IsNullOrEmpty(serviceResult.ConfirmationToken))
            {
                if (string.IsNullOrEmpty(serviceResult.User.Email))
                {
                    return Ok(new { Message = "Registration successful, but an issue occurred with your email address. Please contact support." });
                }

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = serviceResult.User.Id, token = serviceResult.ConfirmationToken, area = "" },
                    protocol: Request.Scheme);

                if (string.IsNullOrEmpty(callbackUrl))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error generating confirmation link. Please contact support." });
                }
                
                try
                {
                    await _emailSender.SendEmailAsync(serviceResult.User.Email,
                        "Confirm your FitnessTracker Account",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    
                    return Ok(new { Message = "Registration successful. Please check your email to confirm your account." });
                }
                catch (System.Exception)
                {
                    return Ok(new { Message = "Registration successful, but we couldn't send the confirmation email. Please try logging in to see if a resend option is available or contact support." });
                }
            }

            if (serviceResult.IdentityResult != null && serviceResult.IdentityResult.Errors.Any())
            {
                foreach (var error in serviceResult.IdentityResult.Errors) { ModelState.AddModelError(error.Code, error.Description); }
            }
            else if (serviceResult.IdentityResult == null || !serviceResult.IdentityResult.Succeeded) 
            {
                 ModelState.AddModelError(string.Empty, "User registration failed for an unknown reason.");
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }
            if (string.IsNullOrEmpty(user.UserName))
            {
                return Unauthorized(new { Message = "Invalid account configuration. Please contact support." });
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password,
                                                                   isPersistent: false,
                                                                   lockoutOnFailure: true);
            if (signInResult.Succeeded)
            {
                var loginResponse = await _userService.GenerateLoginResponseAsync(user);
                return Ok(loginResponse);
            }
            if (signInResult.IsNotAllowed)
            {
                if (!user.EmailConfirmed) 
                {
                    return Unauthorized(new { Message = "Please confirm your email before logging in." });
                }
                return Unauthorized(new { Message = "Account not allowed to sign in. Please contact support." });
            }
            if (signInResult.IsLockedOut)
            {
                return Unauthorized(new { Message = "Account locked out due to multiple failed attempts. Please try again later." });
            }
            return Unauthorized(new { Message = "Invalid email or password." });
        }


        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequestDto requestDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }


            var serviceResult = await _userService.PrepareResendConfirmationAsync(requestDto.Email);

            if (!serviceResult.UserFound)
            {
                return Ok(new { Message = "If an account with that email address exists and requires confirmation, a new verification email has been sent. Please check your inbox (and spam folder)." });
            }
            if (serviceResult.AlreadyConfirmed)
            {
                return Ok(new { Message = "This email address has already been confirmed. You can proceed to log in." });
            }
            if (serviceResult.User != null && !string.IsNullOrEmpty(serviceResult.User.Email) && !string.IsNullOrEmpty(serviceResult.ConfirmationToken))
            {
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = serviceResult.User.Id, token = serviceResult.ConfirmationToken, area = "" },
                    protocol: Request.Scheme); 

                if (string.IsNullOrEmpty(callbackUrl)) 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error generating confirmation link. Please contact support." });
                }
                try
                {
                    await _emailSender.SendEmailAsync(serviceResult.User.Email,
                        "Confirm your FitnessTracker Account (New Link)",
                        $"We received a request to resend the confirmation link for your FitnessTracker account. Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>. If you did not request this, please ignore this email.");
                    
                    return Ok(new { Message = "A new verification email has been sent to your email address. Please check your inbox (and spam folder)." });
                }
                catch (System.Exception) 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "We encountered an issue trying to resend the confirmation email. Please try again later or contact support." });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while processing your request. Please try again." });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(new { UserId = userId, Email = email });
        }
    }
}