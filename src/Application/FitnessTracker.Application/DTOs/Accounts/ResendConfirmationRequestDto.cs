using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Account
{
    public class ResendConfirmationRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}