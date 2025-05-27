using FitnessTracker.Domain.Entities.Nutrition;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [PersonalData]
        [StringLength(50)]
        public string? LastName { get; set; }

        public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();

        [PersonalData]
        [StringLength(2048)]
        public string? ProfilePictureUrl { get; set; }

        [PersonalData]
        [StringLength(500)]
        public string? Bio { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string? Location { get; set; }

        public virtual ICollection<LoggedFoodItem> LoggedFoodItems { get; set; } = new List<LoggedFoodItem>();
        public virtual ICollection<FoodItem> CustomFoodItems { get; set; } = new List<FoodItem>();
        public virtual UserNutritionGoal? NutritionGoal { get; set; }
    }
}