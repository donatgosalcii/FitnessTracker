using FitnessTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessTracker.Domain.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.UserId)
                .IsRequired();

            builder.Property(cm => cm.Message)
                .IsRequired();

            builder.Property(cm => cm.Response)
                .IsRequired();

            builder.Property(cm => cm.Timestamp)
                .IsRequired();

            builder.Property(cm => cm.IsArchived)
                .IsRequired();
        }
    }
} 