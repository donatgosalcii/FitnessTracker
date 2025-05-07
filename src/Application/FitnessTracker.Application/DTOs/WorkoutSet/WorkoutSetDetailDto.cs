namespace FitnessTracker.Application.DTOs.WorkoutSet
{
    public class WorkoutSetDetailDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public int SetNumber { get; set; }
        public int? Reps { get; set; }
        public decimal? Weight { get; set; }
        public int? DurationSeconds { get; set; }
        public decimal? Distance { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}