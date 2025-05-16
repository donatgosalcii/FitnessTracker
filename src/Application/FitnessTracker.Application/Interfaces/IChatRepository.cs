using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageByIdAsync(int id);
        Task<IEnumerable<ChatMessage>> GetUserChatHistoryAsync(string userId);
        Task<bool> DeleteMessageAsync(int id);
    }
} 