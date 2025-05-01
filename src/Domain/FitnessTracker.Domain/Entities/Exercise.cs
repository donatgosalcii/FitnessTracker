using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }

    [Display(Name = "Exercise Name")]
    [StringLength(150)]
    public required string Name { get; set; }

    [StringLength(1000)] 
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    public ICollection<MuscleGroup> MuscleGroups { get; set; } = new List<MuscleGroup>();

    public ICollection<WorkoutSet> WorkoutSets { get; set; } = new List<WorkoutSet>();
}