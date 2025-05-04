using FitnessTracker.Application.DTOs.Account;
using Microsoft.AspNetCore.Identity; 

namespace FitnessTracker.Application.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto);
}