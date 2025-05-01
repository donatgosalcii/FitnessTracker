using System.ComponentModel.DataAnnotations; 

namespace FitnessTracker.Domain.Entities;

public class MuscleGroup
{
    public int Id { get; set; } 

    [Display(Name = "Muscle Group Name")] 
    [StringLength(100)]
    public required string Name { get; set; } 

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}