// src/Domain/FitnessTracker.Domain/Entities/WorkoutSet.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [Column]

namespace FitnessTracker.Domain.Entities;

public class WorkoutSet
{
    public int Id { get; set; } 
    public int ExerciseId { get; set; }
    public int WorkoutId { get; set; }

    [Display(Name = "Set Number")]
    [Range(1, 100, ErrorMessage = "Set number must be between 1 and 100.")]
    public int SetNumber { get; set; } 

    [Display(Name = "Reps")]
    [Range(0, 1000, ErrorMessage = "Reps must be non-negative and reasonable.")]
    public int? Reps { get; set; }

    [Display(Name = "Weight")] 
    [Range(0.0, 10000.0, ErrorMessage = "Weight must be non-negative.")]
    [Column(TypeName = "decimal(6, 2)")] 
    public decimal? Weight { get; set; }

    [Display(Name = "Duration (seconds)")]
    [Range(0, 86400, ErrorMessage = "Duration must be non-negative (max 24 hours in seconds).")] 
    public int? DurationSeconds { get; set; }

    [Display(Name = "Distance")] 
    [Range(0.0, 10000.0, ErrorMessage = "Distance must be non-negative.")] 
    [Column(TypeName = "decimal(8, 2)")]
    public decimal? Distance { get; set; } 
    [StringLength(500)] 
    [DataType(DataType.MultilineText)] 
    public string? Notes { get; set; }

    public Exercise Exercise { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
}