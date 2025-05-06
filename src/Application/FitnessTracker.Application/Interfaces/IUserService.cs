using FitnessTracker.Application.DTOs;
using Microsoft.AspNetCore.Identity; 

namespace FitnessTracker.Application.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto);
    Task<LoginResponseDto> LoginUserAsync(UserLoginDto loginDto);
}