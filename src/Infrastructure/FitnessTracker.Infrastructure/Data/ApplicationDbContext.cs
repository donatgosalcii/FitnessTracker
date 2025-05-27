using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Nutrition;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MuscleGroup> MuscleGroups { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<Workout> Workouts { get; set; } = null!;
        public DbSet<WorkoutSet> WorkoutSets { get; set; } = null!;
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        public DbSet<FoodItem> FoodItems { get; set; } = null!;
        public DbSet<LoggedFoodItem> LoggedFoodItems { get; set; } = null!;
        public DbSet<UserNutritionGoal> UserNutritionGoals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessTracker.Domain.Entities.MuscleGroup).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}