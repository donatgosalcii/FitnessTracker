namespace FitnessTracker.Domain.Entities
{
    public class WorkoutExercise
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public int ExerciseId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal Weight { get; set; }
        
        public virtual Workout? Workout { get; set; }
        public virtual Exercise? Exercise { get; set; }
    }
} 