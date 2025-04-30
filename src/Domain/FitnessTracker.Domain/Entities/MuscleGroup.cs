namespace FitnessTracker.Domain.Entities;

public class MuscleGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}