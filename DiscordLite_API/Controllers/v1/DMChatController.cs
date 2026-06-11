using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordLite_API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DMChatController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IDMChatService _dmChatService;

        public DMChatController(IMessageService messageService, IDMChatService dmChatService)
        {
            _dmChatService = dmChatService;
            _messageService = messageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DMChatDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllChats()
        {
            var result = await _dmChatService.GetChatsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{chatId}")]
        [ProducesResponseType(typeof(ApiResponse<DMChatDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DMChatDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<DMChatDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChat(int chatId)
        {
            var result = await _dmChatService.GetChatAsync(chatId, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(ApiResponse<List<MessageDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<MessageDTO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<List<MessageDTO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessages(int chatId, int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _messageService.GetMessagesAsync(chatId, userId, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}