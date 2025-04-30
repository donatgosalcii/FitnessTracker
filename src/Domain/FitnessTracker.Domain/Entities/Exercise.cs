namespace FitnessTracker.Domain.Entities;

public class Exercise
{
    public int Id { get; set; } // Primary Key
    public required string Name { get; set; }
    public string? Description { get; set; } // Optional description
 
    public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();

    public ICollection<WorkoutSet> WorkoutSets { get; set; } = new List<WorkoutSet>();
}