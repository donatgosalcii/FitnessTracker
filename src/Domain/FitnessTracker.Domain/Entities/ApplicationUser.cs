using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FitnessTracker.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [PersonalData]
    [StringLength(50)]
    public string? LastName { get; set; }

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
