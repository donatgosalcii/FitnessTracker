using System.Threading.Tasks;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessageAsync(string userId, string message);
        Task<ChatMessage> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<ChatMessage>> GetUserChatHistoryAsync(string userId);
    }
} 