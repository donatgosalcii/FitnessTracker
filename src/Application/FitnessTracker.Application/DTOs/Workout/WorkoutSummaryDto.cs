namespace FitnessTracker.Application.DTOs.Workout
{
    public class WorkoutSummaryDto
    {
        public int Id { get; set; }
        public DateTime DatePerformed { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}