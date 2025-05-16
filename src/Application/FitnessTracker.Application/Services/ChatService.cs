using FitnessTracker.Application.DTOs.Chat;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IAIClient _aiClient;

        public ChatService(IChatRepository chatRepository, IAIClient aiClient)
        {
            _chatRepository = chatRepository;
            _aiClient = aiClient;
        }

        public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            try
            {
                string response;

                try
                {
                    response = await _aiClient.GetResponseAsync(request.Message);
                }
                catch (Exception)
                {
                    response = GetFallbackResponse(request.Message);
                }

                var chatMessage = new ChatMessage
                {
                    UserId = request.UserId,
                    Message = request.Message,
                    Response = response,
                    Timestamp = DateTime.UtcNow,
                    IsArchived = false
                };

                var savedMessage = await _chatRepository.AddMessageAsync(chatMessage);

                return new ChatResponseDto
                {
                    Id = savedMessage.Id,
                    Message = savedMessage.Message,
                    Response = savedMessage.Response,
                    Timestamp = savedMessage.Timestamp
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ChatMessageDto?> GetMessageByIdAsync(int messageId)
        {
            var message = await _chatRepository.GetMessageByIdAsync(messageId);
            if (message == null)
                return null;

            return new ChatMessageDto
            {
                Id = message.Id,
                UserId = message.UserId,
                Message = message.Message,
                Response = message.Response,
                Timestamp = message.Timestamp,
                IsArchived = message.IsArchived
            };
        }

        public async Task<IEnumerable<ChatMessageDto>> GetUserChatHistoryAsync(string userId)
        {
            var messageHistory = await _chatRepository.GetUserChatHistoryAsync(userId);
            var result = new List<ChatMessageDto>();

            foreach (var message in messageHistory)
            {
                result.Add(new ChatMessageDto
                {
                    Id = message.Id,
                    UserId = message.UserId,
                    Message = message.Message,
                    Response = message.Response,
                    Timestamp = message.Timestamp,
                    IsArchived = message.IsArchived
                });
            }

            return result;
        }

        private string GetFallbackResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "I didn't receive your message. Could you please try again?";

            string lowercaseMessage = message.ToLower();
            
            if (lowercaseMessage.Contains("workout") || lowercaseMessage.Contains("exercise") || lowercaseMessage.Contains("training"))
                return "For a balanced workout routine, focus on compound exercises like squats, deadlifts, bench press, and rows. Aim for 3-4 training sessions per week with adequate rest between sessions.";
            
            if (lowercaseMessage.Contains("diet") || lowercaseMessage.Contains("nutrition") || lowercaseMessage.Contains("food"))
                return "A balanced diet for fitness should include lean proteins, complex carbohydrates, healthy fats, and plenty of vegetables. Consider your specific goals (muscle gain, weight loss, etc.) when adjusting your caloric intake.";
            
            if (lowercaseMessage.Contains("protein") || lowercaseMessage.Contains("supplement"))
                return "Protein is essential for muscle repair and growth. Aim for about 1.6-2.2g per kg of bodyweight. Good sources include chicken, fish, eggs, dairy, and plant-based options like tofu and legumes.";
            
            if (lowercaseMessage.Contains("cardio") || lowercaseMessage.Contains("running") || lowercaseMessage.Contains("aerobic"))
                return "Cardiovascular exercise improves heart health and endurance. Aim for 150+ minutes of moderate-intensity cardio per week, combined with strength training for optimal fitness results.";
            
            return "I'm your fitness assistant. I can help with workout planning, nutrition advice, and general fitness tips. Feel free to ask specific questions about these topics.";
        }
    }
} 