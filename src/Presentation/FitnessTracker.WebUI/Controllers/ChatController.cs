using FitnessTracker.Application.DTOs.Chat;
using FitnessTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTracker.WebUI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [ValidateAntiForgeryToken]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return BadRequest("Message cannot be empty");
                }

                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var request = new ChatRequestDto
                {
                    UserId = userId,
                    Message = message
                };

                var response = await _chatService.SendMessageAsync(request);
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while processing your message. Please try again." });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var history = await _chatService.GetUserChatHistoryAsync(userId);
                return Ok(history);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving chat history. Please try again." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var message = await _chatService.GetMessageByIdAsync(id);
                
                if (message == null)
                {
                    return NotFound($"Message with ID {id} not found");
                }

                if (message.UserId != userId)
                {
                    return Forbid("You do not have permission to access this message");
                }

                return Ok(message);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the message. Please try again." });
            }
        }
    }
} 