namespace FitnessTracker.Domain.Entities;

public class WorkoutSet
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }
    public int WorkoutId { get; set; }

    public int SetNumber { get; set; }

    public int? Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? DurationSeconds { get; set; } 
    public decimal? Distance { get; set; } 

    public string? Notes { get; set; } 

    public Exercise Exercise { get; set; } = null!; 
    public Workout Workout { get; set; } = null!;  
}