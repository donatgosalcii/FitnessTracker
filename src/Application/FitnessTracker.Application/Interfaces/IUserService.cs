using FitnessTracker.Application.DTOs.Accounts; 
using FitnessTracker.Domain.Entities;      
using Microsoft.AspNetCore.Identity;     

namespace FitnessTracker.Application.Interfaces
{
    public class UserRegistrationResult
    {
        public IdentityResult IdentityResult { get; set; } = IdentityResult.Failed();
        public ApplicationUser? User { get; set; }
        public string? ConfirmationToken { get; set; } 
        public bool RequiresEmailConfirmation { get; set; } = true; 
    }

    public class ResendConfirmationResult
    {
        public bool UserFound { get; set; }
        public bool AlreadyConfirmed { get; set; }
        public ApplicationUser? User { get; set; } 
        public string? ConfirmationToken { get; set; }
    }

    public interface IUserService
    {
        Task<UserRegistrationResult> RegisterUserAndPrepareConfirmationAsync(UserRegistrationDto dto);
        Task<LoginResponseDto> GenerateLoginResponseAsync(ApplicationUser user);
        Task<ResendConfirmationResult> PrepareResendConfirmationAsync(string email);
    }
}