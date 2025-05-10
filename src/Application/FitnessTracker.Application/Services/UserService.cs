using FitnessTracker.Application.DTOs.Accounts;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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

        public async Task<UserRegistrationResult> RegisterUserAndPrepareConfirmationAsync(UserRegistrationDto dto)
        {
            var registrationResultOutput = new UserRegistrationResult();

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                registrationResultOutput.IdentityResult = IdentityResult.Failed(new IdentityError { Code = "EmailInUse", Description = "Email is already in use." });
                return registrationResultOutput;
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            registrationResultOutput.IdentityResult = createResult;
            registrationResultOutput.User = user;

            if (createResult.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                registrationResultOutput.ConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                registrationResultOutput.RequiresEmailConfirmation = true;
            }

            return registrationResultOutput;
        }

        public async Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user)
        {

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

        public async Task<ResendConfirmationResult> PrepareResendConfirmationAsync(string email)
        {
            var resultOutput = new ResendConfirmationResult();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                resultOutput.UserFound = false;
                return resultOutput;
            }

            resultOutput.UserFound = true;
            resultOutput.User = user;

            if (string.IsNullOrEmpty(user.Email))
            {
                 resultOutput.UserFound = false;
                 return resultOutput;
            }

            if (user.EmailConfirmed)
            {
                resultOutput.AlreadyConfirmed = true;
                return resultOutput;
            }

            resultOutput.AlreadyConfirmed = false;
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            resultOutput.ConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            return resultOutput;
        }
    }
}