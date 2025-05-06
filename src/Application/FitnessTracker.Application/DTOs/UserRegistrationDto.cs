using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs;

public class UserRegistrationDto
{
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
}