using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Application.DTOs.WorkoutSet
{
    public class LogWorkoutSetDto
    {
        [Required]
        public int ExerciseId { get; set; }

        [Required]
        [Range(1, 100)] 
        public int SetNumber { get; set; }

        [Range(0, 1000)]
        public int? Reps { get; set; }

        [Range(0, 10000)]
        [DataType(DataType.Currency)] 
        public decimal? Weight { get; set; }

        [Range(0, 36000)] 
        public int? DurationSeconds { get; set; }

        [Range(0, 1000)] 
        public decimal? Distance { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool HasPerformanceMetric() => Reps.HasValue || Weight.HasValue || DurationSeconds.HasValue || Distance.HasValue;

    }
}