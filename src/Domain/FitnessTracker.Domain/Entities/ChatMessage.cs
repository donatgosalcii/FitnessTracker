using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.Domain.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        
        [StringLength(2000)]
        public required string Message { get; set; }
        
        [StringLength(5000)]
        public required string Response { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;
    }
} 