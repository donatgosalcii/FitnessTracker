using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; 

namespace FitnessTracker.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto)
    {
        _logger.LogInformation("Attempting registration for {Email}", registrationDto.Email);
        var existingUser = await _userManager.FindByEmailAsync(registrationDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Email {Email} already exists.", registrationDto.Email);
            return IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists." });
        }

        var newUser = new ApplicationUser
        {
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,
            Email = registrationDto.Email,
            UserName = registrationDto.Email 
        };

        var result = await _userManager.CreateAsync(newUser, registrationDto.Password);

        if (!result.Succeeded)
        {
             _logger.LogError("User creation failed for {Email}. Errors: {Errors}",
                registrationDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result; 
        }

         _logger.LogInformation("User {Email} created successfully.", registrationDto.Email);

        const string defaultRole = "User";
        if (await _roleManager.RoleExistsAsync(defaultRole))
        {
            var roleResult = await _userManager.AddToRoleAsync(newUser, defaultRole);
            if (roleResult.Succeeded)
            {
                _logger.LogInformation("Assigned '{Role}' role to {Email}.", defaultRole, registrationDto.Email);
            }
            else
            {
                 _logger.LogError("Failed to assign '{Role}' role to {Email}. Errors: {Errors}",
                    defaultRole, registrationDto.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
             _logger.LogWarning("Default role '{Role}' not found. Skipping role assignment for {Email}.", defaultRole, registrationDto.Email);
        }

        return result;
    }
}