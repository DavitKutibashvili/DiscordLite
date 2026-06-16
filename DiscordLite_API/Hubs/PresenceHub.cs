using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DiscordLite_API.Hubs
{
    public class PresenceHub : Hub
    {
        private readonly IPresenceService _presenceService;
        private readonly IFriendshipService _friendshipService;
        private string CurrentUserId => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        public PresenceHub(IFriendshipService friendshipService, IPresenceService presenceService)
        {
            _friendshipService = friendshipService;
            _presenceService = presenceService;
        }
        public async override Task OnConnectedAsync()
        {
            _presenceService.UserConnected(CurrentUserId);
            var friends = await _friendshipService.GetFriendsAsync(CurrentUserId);
            var friendDTOs = friends.Data.Select(f => new FriendDTO
            {
                FriendshipId = f.Id,
                FriendId = f.RequestedById == CurrentUserId ? f.ReceivedById : f.RequestedById,
                FriendUserName = f.RequestedById == CurrentUserId ? f.ReceivedByUsername : f.RequestedByUsername,
                FriendDisplayName = f.RequestedById == CurrentUserId ? f.ReceivedByDisplayName : f.RequestedByDisplayName
            }).ToList();
            var friendIds = friendDTOs.Select(f => f.FriendId);
            await Clients.Users(friendIds).SendAsync("UserOnline", CurrentUserId);
            await Clients.Caller.SendAsync("UserOnline", CurrentUserId);
            var onlineFriendIds = _presenceService.GetOnlineUsers(friendIds);
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", onlineFriendIds);
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var isLastConnection = _presenceService.UserDisconnected(CurrentUserId);
            if (isLastConnection)
            {
                var friends = await _friendshipService.GetFriendsAsync(CurrentUserId);
                var friendDTOs = friends.Data.Select(f => new FriendDTO
                {
                    FriendshipId = f.Id,
                    FriendId = f.RequestedById == CurrentUserId ? f.ReceivedById : f.RequestedById,
                    FriendUserName = f.RequestedById == CurrentUserId ? f.ReceivedByUsername : f.RequestedByUsername,
                    FriendDisplayName = f.RequestedById == CurrentUserId ? f.ReceivedByDisplayName : f.RequestedByDisplayName
                }).ToList();
                var onlineFriendIds = _presenceService.GetOnlineUsers(friendDTOs.Select(f => f.FriendId));
                await Clients.Users(onlineFriendIds).SendAsync("UserOffline", CurrentUserId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
