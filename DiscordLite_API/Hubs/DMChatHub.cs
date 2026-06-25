using DiscordLite_API.Data;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordLite_API.Hubs
{
    [Authorize]
    public class DMChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ApplicationDbContext _db;
        private string CurrentUserId => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        public DMChatHub(IMessageService messageService, ApplicationDbContext applicationDbContext, IPresenceService presenceService, IFriendshipService friendshipService)
        {
            _messageService = messageService;
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
    }
}
