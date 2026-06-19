
namespace DiscordLite_API.Model
{
    public class ChannelMessage : BaseMessage
    {
        public int ChannelId { get; set; }
        public ServerChannel Channel { get; set; } = null!;
    }
}
