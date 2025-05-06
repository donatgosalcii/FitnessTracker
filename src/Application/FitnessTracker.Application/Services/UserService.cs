using FitnessTracker.Application.DTOs;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserService> logger,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", dto.Email);
                return IdentityResult.Failed(new IdentityError { Code = "EmailInUse", Description = "Email is already in use." });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully.", dto.Email);
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                    _logger.LogInformation("Role 'User' created.");
                }
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation("User {Email} added to 'User' role.", dto.Email);
            }
            else
            {
                _logger.LogError("User creation failed for {Email}. Errors: {Errors}", dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return result;
        }

        public async Task<LoginResponseDto> LoginUserAsync(UserLoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}.", loginDto.Email);
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid email or password." };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login failed: User {Email} is locked out.", loginDto.Email);
                return new LoginResponseDto { IsSuccess = false, Message = "Account locked out. Please try again later." };
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (isPasswordCorrect)
            {
                _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
                if (await _userManager.GetAccessFailedCountAsync(user) > 0)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                var tokenDetails = await _jwtTokenGenerator.GenerateTokenDetailsAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);

                return new LoginResponseDto
                {
                    IsSuccess = true,
                    Message = "Login successful.",
                    Token = tokenDetails.Token,
                    ExpiresOn = tokenDetails.ExpiresOn,
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = userRoles ?? new List<string>()
                };
            }
            else
            {
                _logger.LogWarning("Login failed for user {Email}: Invalid credentials.", loginDto.Email);
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("User {Email} is now locked out after failed login attempt.", loginDto.Email);
                    return new LoginResponseDto { IsSuccess = false, Message = "Account locked out due to multiple failed attempts. Please try again later." };
                }

                return new LoginResponseDto { IsSuccess = false, Message = "Invalid email or password." };
            }
        }
    }
}