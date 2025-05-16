using FitnessTracker.Application.DTOs.Chat;

namespace FitnessTracker.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request);
        Task<ChatMessageDto?> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<ChatMessageDto>> GetUserChatHistoryAsync(string userId);
    }
} 