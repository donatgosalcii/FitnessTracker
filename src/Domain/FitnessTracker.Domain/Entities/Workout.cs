using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Domain.Entities;

public class Workout
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    [Display(Name = "Date Performed")]
    [DataType(DataType.DateTime)] 
    public DateTime DatePerformed { get; set; }

    [StringLength(1000)] 
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();

}