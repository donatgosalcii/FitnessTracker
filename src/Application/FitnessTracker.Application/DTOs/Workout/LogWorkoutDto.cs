using FitnessTracker.Application.DTOs.WorkoutSet;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Workout
{
    public class LogWorkoutDto
    {
        [Required]
        public DateTime DatePerformed { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Workout must contain at least one set.")]
        public List<LogWorkoutSetDto> Sets { get; set; } = new List<LogWorkoutSetDto>();
    }
}