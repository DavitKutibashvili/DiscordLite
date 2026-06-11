namespace DiscordLite_WEB.ViewModels
{
    public class OpenDMChatVM
    {
        public int ChatId { get; set; }
        public string OtherUserId { get; set; } = null!;
        public string OtherUserName { get; set; } = null!;
        public string OtherDisplayName { get; set; } = null!;
    }
}
