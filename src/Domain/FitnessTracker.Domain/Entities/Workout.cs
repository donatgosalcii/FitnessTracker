namespace FitnessTracker.Domain.Entities;

public class Workout
{
    public int Id { get; set; }

    public DateTime DatePerformed { get; set; }
    public string? Notes { get; set; } 

    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}