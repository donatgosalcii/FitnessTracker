using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.DTOs.User;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Http; 
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

         public async Task<Result<(ApplicationUser User, string ConfirmationToken)>> RegisterUserAsync(UserRegistrationDto dto) { var existingUser = await _userManager.FindByEmailAsync(dto.Email); if (existingUser != null) { return Result<(ApplicationUser User, string ConfirmationToken)>.Conflict($"An account with email '{dto.Email}' already exists.", "EmailInUse"); } var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName, EmailConfirmed = false }; var createResult = await _userManager.CreateAsync(user, dto.Password); if (!createResult.Succeeded) { var err = string.Join(" ", createResult.Errors.Select(e => e.Description)); return Result<(ApplicationUser User, string ConfirmationToken)>.ValidationFailed(err, createResult.Errors.FirstOrDefault()?.Code ?? "RegFail"); } if (!await _roleManager.RoleExistsAsync("User")) { await _roleManager.CreateAsync(new IdentityRole("User")); } await _userManager.AddToRoleAsync(user, "User"); var token = await _userManager.GenerateEmailConfirmationTokenAsync(user); return Result<(ApplicationUser User, string ConfirmationToken)>.Success((user, token)); }
         public async Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user) { var tokenDetails = await _jwtTokenGenerator.GenerateTokenDetailsAsync(user); var userRoles = await _userManager.GetRolesAsync(user); return new LoginResponseDto { IsSuccess = true, Message = "Login successful.", Token = tokenDetails.Token, ExpiresOn = tokenDetails.ExpiresOn, UserId = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, Roles = userRoles ?? new List<string>() }; }
         public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto) { var user = await _userManager.FindByIdAsync(userId); if (user == null) { return Result.NotFound("User not found."); } var changeResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword); if (changeResult.Succeeded) { await _userManager.UpdateSecurityStampAsync(user); return Result.Success(); } var err = string.Join(" ", changeResult.Errors.Select(e => e.Description)); return changeResult.Errors.Any(e => e.Code == "PasswordMismatch") ? Result.ValidationFailed("Current password incorrect.", "PwdMismatch") : Result.ValidationFailed(err, "PwdChangeFail"); }
         public async Task<Result<(ApplicationUser User, string ResetToken)>> GeneratePasswordResetTokenAsync(string email) { var user = await _userManager.FindByEmailAsync(email); if (user == null) { return Result<(ApplicationUser User, string ResetToken)>.NotFound("User not found."); } var token = await _userManager.GeneratePasswordResetTokenAsync(user); return Result<(ApplicationUser User, string ResetToken)>.Success((user, token)); }
         public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto) { var user = await _userManager.FindByEmailAsync(dto.Email); if (user == null) { return Result.NotFound("User not found."); } var resetResult = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password); if (resetResult.Succeeded) { await _userManager.UpdateSecurityStampAsync(user); return Result.Success(); } var err = string.Join(" ", resetResult.Errors.Select(e => e.Description)); return Result.ValidationFailed(err, resetResult.Errors.FirstOrDefault()?.Code ?? "PwdResetFail"); }
         public async Task<Result<(ApplicationUser User, string ConfirmationToken)>> PrepareResendConfirmationTokenAsync(string email) { var user = await _userManager.FindByEmailAsync(email); if (user == null) { return Result<(ApplicationUser User, string ConfirmationToken)>.NotFound("User not found."); } if (string.IsNullOrEmpty(user.Email)) { return Result<(ApplicationUser User, string ConfirmationToken)>.Failure("User account has invalid email data.", ErrorType.Unexpected); } if (user.EmailConfirmed) { return Result<(ApplicationUser User, string ConfirmationToken)>.Conflict("This email address has already been confirmed.", "EmailAlreadyConfirmed"); } var token = await _userManager.GenerateEmailConfirmationTokenAsync(user); return Result<(ApplicationUser User, string ConfirmationToken)>.Success((user, token)); }
         public async Task<Result<UserProfileDto>> GetUserProfileByIdAsync(string targetUserId, string? requestingUserId = null) { try { var user = await _userManager.FindByIdAsync(targetUserId); if (user == null) { return Result<UserProfileDto>.NotFound($"User profile not found."); } var userProfileDto = new UserProfileDto { Id = user.Id, UserName = user.UserName ?? "", Email = user.Email ?? "", FirstName = user.FirstName ?? "", LastName = user.LastName ?? "", ProfilePictureUrl = user.ProfilePictureUrl, Bio = user.Bio, Location = user.Location }; return Result<UserProfileDto>.Success(userProfileDto); } catch (System.Exception ex) { return Result<UserProfileDto>.Unexpected($"An error occurred: {ex.Message}"); } }

        public async Task<Result<UserProfileDto>> UpdateMyProfileAsync(string currentUserId, UpdateUserProfileDto dto, IFormFile? profilePictureFile)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    return Result<UserProfileDto>.NotFound("Current user not found. Unable to update profile.");
                }

                bool hasChanges = false;
                string? newCalculatedProfilePictureUrl = user.ProfilePictureUrl;

                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(profilePictureFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return Result<UserProfileDto>.ValidationFailed("Invalid file type. Only JPG, PNG, GIF are allowed.");
                    }
                    if (profilePictureFile.Length > 2 * 1024 * 1024) 
                    {
                        return Result<UserProfileDto>.ValidationFailed("File size exceeds limit of 2MB.");
                    }

                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadsFolderPath = Path.Combine(webRootPath, "images", "profiles");
                    
                    if (!Directory.Exists(uploadsFolderPath)) { Directory.CreateDirectory(uploadsFolderPath); }

                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.StartsWith("/images/profiles/"))
                    {
                        var oldFileName = Path.GetFileName(user.ProfilePictureUrl);
                        var oldFilePath = Path.Combine(uploadsFolderPath, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try { System.IO.File.Delete(oldFilePath); } catch { }
                        }
                    }
                    
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePictureFile.CopyToAsync(stream);
                    }
                    newCalculatedProfilePictureUrl = $"/images/profiles/{uniqueFileName}";
                }

                if (dto.FirstName != null && user.FirstName != dto.FirstName) { user.FirstName = dto.FirstName; hasChanges = true; }
                if (dto.LastName != null && user.LastName != dto.LastName) { user.LastName = dto.LastName; hasChanges = true; }
                if (dto.Bio != null && user.Bio != dto.Bio) { user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio; hasChanges = true; }
                if (dto.Location != null && user.Location != dto.Location) { user.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location; hasChanges = true; }

                if (user.ProfilePictureUrl != newCalculatedProfilePictureUrl) {
                    user.ProfilePictureUrl = newCalculatedProfilePictureUrl;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    var currentProfileDtoNoChange = new UserProfileDto { Id = user.Id, UserName = user.UserName ?? "", Email = user.Email ?? "", FirstName = user.FirstName ?? "", LastName = user.LastName ?? "", ProfilePictureUrl = user.ProfilePictureUrl, Bio = user.Bio, Location = user.Location };
                    return Result<UserProfileDto>.Success(currentProfileDtoNoChange);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errorMessages = string.Join(" ", updateResult.Errors.Select(e => e.Description));
                    return Result<UserProfileDto>.ValidationFailed(errorMessages, "ProfileUpdateFailure");
                }

                var updatedUser = await _userManager.FindByIdAsync(currentUserId);
                if (updatedUser == null) { return Result<UserProfileDto>.Unexpected("Failed to retrieve profile after update."); }
                
                var updatedProfileDto = new UserProfileDto { Id = updatedUser.Id, UserName = updatedUser.UserName ?? "", Email = updatedUser.Email ?? "", FirstName = updatedUser.FirstName ?? "", LastName = updatedUser.LastName ?? "", ProfilePictureUrl = updatedUser.ProfilePictureUrl, Bio = updatedUser.Bio, Location = updatedUser.Location };
                return Result<UserProfileDto>.Success(updatedProfileDto);
            }
            catch (System.Exception ex)
            {
                return Result<UserProfileDto>.Unexpected($"An error occurred while updating profile: {ex.Message}");
            }
        }
    }
}