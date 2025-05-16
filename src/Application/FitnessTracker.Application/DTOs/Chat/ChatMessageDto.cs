namespace FitnessTracker.Application.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsArchived { get; set; }
    }
} 