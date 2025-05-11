using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FitnessTracker.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<(ApplicationUser User, string ConfirmationToken)>> RegisterUserAsync(UserRegistrationDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return Result<(ApplicationUser User, string ConfirmationToken)>.Conflict(
                    $"An account with email '{dto.Email}' already exists.", "EmailInUse");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email, Email = dto.Email, FirstName = dto.FirstName,
                LastName = dto.LastName, EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errorMessages = string.Join(" ", createResult.Errors.Select(e => e.Description));
                return Result<(ApplicationUser User, string ConfirmationToken)>.ValidationFailed(
                    errorMessages, createResult.Errors.FirstOrDefault()?.Code ?? "RegistrationFailure");
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User")); 
            }
            await _userManager.AddToRoleAsync(user, "User"); 

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return Result<(ApplicationUser User, string ConfirmationToken)>.Success((user, token));
        }

        public async Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user)
        {
            var tokenDetails = await _jwtTokenGenerator.GenerateTokenDetailsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            return new LoginResponseDto
            {
                IsSuccess = true, Message = "Login successful.", Token = tokenDetails.Token,
                ExpiresOn = tokenDetails.ExpiresOn, UserId = user.Id, Email = user.Email,
                FirstName = user.FirstName, LastName = user.LastName, Roles = userRoles ?? new List<string>()
            };
        }
        
        public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.NotFound("User not found for password change.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return Result.Success();
            }
            
            var errorMessages = string.Join(" ", changePasswordResult.Errors.Select(e => e.Description));
            if (changePasswordResult.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return Result.ValidationFailed("The current password entered is incorrect.", "PasswordMismatch");
            }
            return Result.ValidationFailed(errorMessages, "PasswordChangeFailed");
        }

        public async Task<Result<(ApplicationUser User, string ResetToken)>> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<(ApplicationUser User, string ResetToken)>.NotFound("User with this email not found.");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<(ApplicationUser User, string ResetToken)>.Success((user, token));
        }
        
        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result.NotFound("User not found for password reset.");
            }
            var resetResult = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
            if (resetResult.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return Result.Success();
            }
            var errorMessages = string.Join(" ", resetResult.Errors.Select(e => e.Description));
            return Result.ValidationFailed(errorMessages, resetResult.Errors.FirstOrDefault()?.Code ?? "PasswordResetFailed");
        }

        public async Task<Result<(ApplicationUser User, string ConfirmationToken)>> PrepareResendConfirmationTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<(ApplicationUser User, string ConfirmationToken)>.NotFound("User not found.");
            }
            if (string.IsNullOrEmpty(user.Email)) 
            {
                 return Result<(ApplicationUser User, string ConfirmationToken)>.Failure("User account has invalid email data.", ErrorType.Unexpected);
            }
            if (user.EmailConfirmed)
            {
                return Result<(ApplicationUser User, string ConfirmationToken)>.Conflict("This email address has already been confirmed.", "EmailAlreadyConfirmed");
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return Result<(ApplicationUser User, string ConfirmationToken)>.Success((user, token));
        }
    }
}