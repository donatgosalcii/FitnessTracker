using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName)
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .HasMaxLength(50);

            builder.HasMany(u => u.Workouts)
                .WithOne() 
                .HasForeignKey("UserId")
                .IsRequired(); 

            builder.Property(u => u.ProfilePictureUrl)
                .HasMaxLength(2048);

            builder.Property(u => u.Bio)
                .HasMaxLength(500);

            builder.Property(u => u.Location)
                .HasMaxLength(100);
        }
    }
}