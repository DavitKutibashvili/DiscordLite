using DiscordLite_API.Data;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordLite_API.Hubs
{
    public class DMChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IPresenceService _presenceService;
        private readonly IFriendshipService _friendshipService;
        private readonly ApplicationDbContext _db;
        private string CurrentUserId => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        public DMChatHub(IMessageService messageService, ApplicationDbContext applicationDbContext, IPresenceService presenceService, IFriendshipService friendshipService)
        {
            _messageService = messageService;
            _presenceService = presenceService;
            _friendshipService = friendshipService;
            _db = applicationDbContext;
        }
        public async Task StartTyping(int chatId)
        {
            var chat = await _db.DirectMessageChats.FindAsync(chatId);
            if (chat == null) return;

            var otherUserId = chat.User1Id == CurrentUserId ? chat.User2Id : chat.User1Id;

            await Clients.User(otherUserId).SendAsync("UserTyping", CurrentUserId);
        }
        public async Task StopTyping(int chatId)
        {
            var chat = await _db.DirectMessageChats.FindAsync(chatId);
            if (chat == null) return;
            var otherUserId = chat.User1Id == CurrentUserId ? chat.User2Id : chat.User1Id;
            await Clients.User(otherUserId).SendAsync("UserStoppedTyping", CurrentUserId);
        }
        public async Task SendMessage(int chatId, string content)
        {
            var result = await _messageService.SendMessageAsync(chatId, CurrentUserId, content);
            if (!result.Success || result.Data == null) return;

            var chat = await _db.DirectMessageChats.FindAsync(chatId);
            if (chat == null) return;

            var otherUserId = chat.User1Id == CurrentUserId ? chat.User2Id : chat.User1Id;

            var payload = new
            {
                id = result.Data.Id,
                senderId = result.Data.SenderId,
                content = result.Data.Content,
                sentAt = result.Data.SentAt
            };

            await Clients.User(otherUserId).SendAsync("ReceiveMessage", payload);
            await Clients.Caller.SendAsync("ReceiveMessage", payload);
        }
        public async override Task OnConnectedAsync()
        {
            _presenceService.UserConnected(CurrentUserId);
            var otherUserId = Context.GetHttpContext()?.Request.Query["otherUserId"].ToString();
            if (otherUserId == null) return;
            if (_presenceService.IsOnline(otherUserId))
                await Clients.Caller.SendAsync("UserOnline", otherUserId);
            await Clients.User(otherUserId).SendAsync("UserOnline", CurrentUserId);
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var wentOffline = _presenceService.UserDisconnected(CurrentUserId);
            var otherUserId = Context.GetHttpContext()?.Request.Query["otherUserId"].ToString();

            if (wentOffline && otherUserId != null)
                await Clients.User(otherUserId).SendAsync("UserOffline", CurrentUserId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
