using DiscordLite_API.Data;
using DiscordLite_API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordLite_API.Hubs
{
    [Authorize]
    public class ChannelHub : Hub
    {
        private readonly IChannelMessageService _channelMessageService;
        private readonly ApplicationDbContext _db;
        private string CurrentUserId => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        public ChannelHub(IChannelMessageService channelMessageService, ApplicationDbContext applicationDbContext)
        {
            _channelMessageService = channelMessageService;
            _db = applicationDbContext;
        }

        public async Task JoinChannel(int channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel-{channelId}");
        }
        public async Task LeaveChannel(int channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel-{channelId}");
        }
        public async Task SendMessage(int channelId, string content)
        {
            var response = await _channelMessageService.SendMessageAsync(channelId, CurrentUserId, content);
            if (!response.Success || response.Data == null) return;

            var payload = new
            {
                id = response.Data.Id,
                senderId = response.Data.SenderId,
                senderDisplayName = response.Data.SenderDisplayName,
                content = response.Data.Content,
                sentAt = response.Data.SentAt
            };

            await Clients.Group($"channel-{channelId}").SendAsync("ReceiveMessage", payload);
        }
    }
}
