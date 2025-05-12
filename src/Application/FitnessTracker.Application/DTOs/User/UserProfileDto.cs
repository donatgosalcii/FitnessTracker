namespace FitnessTracker.Application.DTOs.User
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }

    }
}