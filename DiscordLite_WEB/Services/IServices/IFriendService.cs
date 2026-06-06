using DiscordLite_DTO;

namespace DiscordLite_WEB.Services.IServices
{
    public interface IFriendService
    {
        Task<T> GetFriendsAsync<T>();
        Task<T> GetFriendRequestsAsync<T>();
        Task<T> SendFriendRequestAsync<T>(string username);
        Task<T> AcceptFriendRequestAsync<T>(int requestId);
        Task<T> DeclineFriendRequestAsync<T>(int requestId);
        Task<T> RemoveFriendAsync<T>(int friendshipId);
    }
}
