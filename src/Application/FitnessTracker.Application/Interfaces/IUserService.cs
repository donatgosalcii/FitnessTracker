using FitnessTracker.Application.Common; 
using FitnessTracker.Application.DTOs.Accounts; 
using FitnessTracker.Domain.Entities;      

namespace FitnessTracker.Application.Interfaces
{

    public interface IUserService
    {
      
        Task<Result<(ApplicationUser User, string ConfirmationToken)>> RegisterUserAsync(UserRegistrationDto dto);

        Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user); // Stays the same

        Task<Result<(ApplicationUser User, string ConfirmationToken)>> PrepareResendConfirmationTokenAsync(string email);
    }
}