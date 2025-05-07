using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Exercise;

public class CreateExerciseDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)] public string Description { get; set; } = string.Empty;

    public List<int> MuscleGroupIds { get; set; } = new();
}