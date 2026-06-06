using DiscordLite_DTO;

namespace DiscordLite_WEB.ViewModels
{
    public class FriendshipVM
    {
        public List<FriendRequestDTO> PendingFriendRequests { get; set; } = new();
        public List<FriendDTO> Friends { get; set; } = new();
    }
}
