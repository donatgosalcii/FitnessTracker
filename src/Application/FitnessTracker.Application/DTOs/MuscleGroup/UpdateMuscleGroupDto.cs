using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.MuscleGroup
{
    public class UpdateMuscleGroupDto
    {
        [Required(ErrorMessage = "Muscle group name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}