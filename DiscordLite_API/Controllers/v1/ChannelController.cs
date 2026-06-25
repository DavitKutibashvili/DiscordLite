using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using DiscordLite_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace DiscordLite_API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelService _channelService;
        private readonly IChannelMessageService _channelMessageService;
        public ChannelController(IChannelMessageService channelMessageService, IChannelService channelService)
        {
            _channelService = channelService;
            _channelMessageService = channelMessageService;
        }

        [HttpPost]
        public async Task<IActionResult> CreteChannel([FromBody] ChannelCreateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,"User is not authorized");
            }
            var response = await _channelService.CreateChannelAsync(dto.ServerId, dto.Name, dto.Type, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteChannel([FromQuery]int channelId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401, "User is not authorized");
            }
            var response = await _channelService.DeleteChannelAsync(channelId, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetChannelMessages([FromQuery] int channelId,[FromQuery] int page = 1,[FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401, "User is not authorized");
            }
            var response = await _channelMessageService.GetMessagesAsync(channelId, userId, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }
    }
}
