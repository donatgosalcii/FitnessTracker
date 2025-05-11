using FitnessTracker.Application.Common;
using FitnessTracker.Application.DTOs.Account;
using FitnessTracker.Application.DTOs.Accounts; 
using FitnessTracker.Domain.Entities;      

namespace FitnessTracker.Application.Interfaces
{

    public interface IUserService
    {
      
        Task<Result<(ApplicationUser User, string ConfirmationToken)>> RegisterUserAsync(UserRegistrationDto dto);

        Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user);

        Task<Result<(ApplicationUser User, string ConfirmationToken)>> PrepareResendConfirmationTokenAsync(string email);
        
        Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        
        Task<Result<(ApplicationUser User, string ResetToken)>> GeneratePasswordResetTokenAsync(string email);
        
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    }
}