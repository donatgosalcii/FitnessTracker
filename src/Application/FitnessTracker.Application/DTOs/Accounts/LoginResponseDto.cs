namespace FitnessTracker.Application.DTOs.Accounts
{
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IList<string>? Roles { get; set; }
    }
}