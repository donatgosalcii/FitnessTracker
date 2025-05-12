using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.User
{
    public class UpdateUserProfileDto
    {
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }
    }
}