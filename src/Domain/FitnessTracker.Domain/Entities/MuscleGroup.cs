using System.ComponentModel.DataAnnotations; // Add this

namespace FitnessTracker.Domain.Entities;

public class MuscleGroup
{
    public int Id { get; set; } // Primary Key

    [Display(Name = "Muscle Group Name")] 
    [StringLength(100)]
    public required string Name { get; set; } 

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}