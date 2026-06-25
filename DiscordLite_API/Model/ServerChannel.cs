using DiscordLite_Utility;
using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public class ServerChannel
    {
        public int Id { get; set; }

        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public int Position { get; set; } = 0;

        public SD.ChannelType Type { get; set; } = SD.ChannelType.Text;

        public List<ChannelMessage> Messages { get; set; } = new();
    }
}
