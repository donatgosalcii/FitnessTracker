using FitnessTracker.Application.DTOs.WorkoutSet;

namespace FitnessTracker.Application.DTOs.Workout
{
    public class WorkoutDetailDto
    {
        public int Id { get; set; }
        public DateTime DatePerformed { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<WorkoutSetDetailDto> Sets { get; set; } = new List<WorkoutSetDetailDto>();
    }
}