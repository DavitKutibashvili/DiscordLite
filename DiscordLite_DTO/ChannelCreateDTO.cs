using DiscordLite_Utility;

namespace DiscordLite_DTO
{
    public class ChannelCreateDTO
    {
        public int ServerId { get; set; }
        public string Name { get; set; } = null!;
        public SD.ChannelType Type { get; set; } = SD.ChannelType.Text;
    }
}
