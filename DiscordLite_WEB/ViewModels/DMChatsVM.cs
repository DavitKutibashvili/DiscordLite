using DiscordLite_DTO;

namespace DiscordLite_WEB.ViewModels
{
    public class DMChatsVM
    {
        public List<DMChatDTO> DMChats { get; set; } = new();
        public string CurrentUserId { get; set; } = null!;
    }
}
