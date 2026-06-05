using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IFriendshipService
    {
        Task<ApiResponse<FriendshipDTO>> SendFriendRequestAsync(string requestedById, string receivedByUsername);
        Task<ApiResponse<FriendshipDTO>> AcceptFriendRequestAsync(int friendshipId, string currentUserId);
        Task<ApiResponse<FriendshipDTO>> DeclineFriendRequestAsync(int friendshipId, string currentUserId);
        Task<ApiResponse<FriendshipDTO>> RemoveFriendAsync(int friendshipId, string currentUserId);
        Task<ApiResponse<FriendshipDTO>> BlockUserAsync(int friendshipId, string currentUserId);
        Task<ApiResponse<List<FriendshipDTO>>> GetFriendsAsync(string currentUserId);
        Task<ApiResponse<List<FriendshipDTO>>> GetPendingRequestsAsync(string currentUserId);
    }
}
