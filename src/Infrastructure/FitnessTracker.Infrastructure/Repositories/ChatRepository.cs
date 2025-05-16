using Microsoft.EntityFrameworkCore;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int id)
        {
            return await _context.ChatMessages.FindAsync(id);
        }

        public async Task<IEnumerable<ChatMessage>> GetUserChatHistoryAsync(string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message == null)
                return false;

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 