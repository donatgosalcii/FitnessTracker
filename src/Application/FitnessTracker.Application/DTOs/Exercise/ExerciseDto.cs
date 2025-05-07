using FitnessTracker.Application.DTOs.MuscleGroup; 

namespace FitnessTracker.Application.DTOs.Exercise
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<MuscleGroupDto> MuscleGroups { get; set; } = new List<MuscleGroupDto>();
    }
}