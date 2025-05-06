using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public class TokenResult 
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
    }

    public interface IJwtTokenGenerator
    {
        Task<TokenResult> GenerateTokenDetailsAsync(ApplicationUser user);
    }
}