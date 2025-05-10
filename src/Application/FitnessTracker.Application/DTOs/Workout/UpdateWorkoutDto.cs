using FitnessTracker.Application.DTOs.WorkoutSet; 
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.Workout
{
    public class UpdateWorkoutDto
    {
        [Required]
        public DateTime DatePerformed { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
        public List<LogWorkoutSetDto> Sets { get; set; } = new List<LogWorkoutSetDto>();
    }
}