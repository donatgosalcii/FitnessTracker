using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
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
            IEmailSender emailSender
            )
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
            var serviceResult = await _userService.RegisterUserAsync(registrationDto);
            if (serviceResult.IsSuccess)
            {
                var (user, rawToken) = serviceResult.Value;
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(rawToken)) { return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Internal error during registration processing." });}
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
                var callbackUrl = Url.Page("/Account/ConfirmEmail", null, new { userId = user.Id, token = encodedToken, area = "" }, Request.Scheme);
                if (string.IsNullOrEmpty(callbackUrl)) { return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error generating confirmation link." });}
                try { await _emailSender.SendEmailAsync(user.Email, "Confirm your FitnessTracker Account", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."); return Ok(new { Message = "Registration successful. Please check your email to confirm your account." }); }
                catch (System.Exception) { return Ok(new { Message = "Registration successful, but we couldn't send the confirmation email. Please try resending it later." });}
            }
            var errorMessage = serviceResult.Error?.Message ?? "User registration failed.";
            return serviceResult.ErrorType switch { ErrorType.Conflict => Conflict(new { Message = errorMessage, Code = serviceResult.Error?.Code }), ErrorType.Validation or _ => BadRequest(new { Message = errorMessage, Code = serviceResult.Error?.Code })};
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) { return Unauthorized(new { Message = "Invalid email or password." }); }
            if (string.IsNullOrEmpty(user.UserName)) { return Unauthorized(new { Message = "Invalid account configuration." }); }
            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, false, true);
            if (signInResult.Succeeded) { var loginResponse = await _userService.GenerateLoginResponseAsync(user); return Ok(loginResponse); }
            if (signInResult.IsNotAllowed) { return Unauthorized(new { Message = !user.EmailConfirmed ? "Please confirm your email before logging in." : "Account not allowed to sign in." }); }
            if (signInResult.IsLockedOut) { return Unauthorized(new { Message = "Account locked out." }); }
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return Unauthorized(new { Message = "User ID not found." }); }
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);
            if (result.IsSuccess) { return Ok(new { Message = "Password changed successfully." }); }
            var errorMessage = result.Error?.Message ?? "Password change failed.";
            return result.ErrorType switch { ErrorType.NotFound => NotFound(new { Message = errorMessage }), ErrorType.Validation => BadRequest(new { Message = errorMessage, Code = result.Error?.Code }), ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." })};
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            string successUserMessage = "If an account with that email exists, a password reset link has been sent. Please check your inbox (and spam folder).";
            var serviceResult = await _userService.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);

            if (serviceResult.IsSuccess)
            {
                var (user, rawToken) = serviceResult.Value;
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(rawToken))
                {
                    return Ok(new { Message = successUserMessage }); 
                }

                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
                var callbackUrl = Url.Page("/Account/ResetPassword", null,
                    new { token = encodedToken, email = user.Email, area = "" }, Request.Scheme);

                if (string.IsNullOrEmpty(callbackUrl))
                {
                    return Ok(new { Message = successUserMessage }); 
                }

                try
                {
                    await _emailSender.SendEmailAsync(user.Email, "Reset your FitnessTracker Password",
                        $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                }
                catch (System.Exception) {  }
            }
            return Ok(new { Message = successUserMessage });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var result = await _userService.ResetPasswordAsync(resetPasswordDto);
            if (result.IsSuccess) { return Ok(new { Message = "Your password has been reset successfully. You can now log in." }); }
            var errorMessage = result.Error?.Message ?? "Password reset failed.";
            return result.ErrorType switch { ErrorType.NotFound => BadRequest(new { Message = "Password reset failed. The user or token may be invalid." }), ErrorType.Validation => BadRequest(new { Message = errorMessage, Code = result.Error?.Code }), ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." })};
        }

        [HttpPost("resend-confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequestDto requestDto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var serviceResult = await _userService.PrepareResendConfirmationTokenAsync(requestDto.Email);
            if (serviceResult.IsSuccess)
            {
                var (user, rawToken) = serviceResult.Value;
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(rawToken)) { return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error preparing data for resend." }); }
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
                var callbackUrl = Url.Page("/Account/ConfirmEmail", null, new { userId = user.Id, token = encodedToken, area = "" }, Request.Scheme);
                if (string.IsNullOrEmpty(callbackUrl))  { return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error generating confirmation link." }); }
                try { await _emailSender.SendEmailAsync(user.Email, "Confirm your FitnessTracker Account (New Link)", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."); return Ok(new { Message = "A new verification email has been sent. Please check your inbox." }); }
                catch (System.Exception)  { return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Could not resend confirmation email." }); }
            }
            var errorMessage = serviceResult.Error?.Message ?? "An error occurred.";
            return serviceResult.ErrorType switch { ErrorType.NotFound => Ok(new { Message = "If an account with that email address exists and requires confirmation, a new verification email has been sent." }), ErrorType.Conflict when serviceResult.Error?.Code == "EmailAlreadyConfirmed" => Ok(new { Message = errorMessage }), ErrorType.Unexpected or _ => StatusCode(StatusCodes.Status500InternalServerError, new { Message = errorMessage })};
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