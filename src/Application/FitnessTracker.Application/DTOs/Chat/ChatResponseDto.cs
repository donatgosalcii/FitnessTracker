namespace FitnessTracker.Application.DTOs.Chat
{
    public class ChatResponseDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
} 