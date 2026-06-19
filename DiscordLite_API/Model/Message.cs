using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public class Message : BaseMessage
    {
        public int ChatId { get; set; }
        public DirectMessageChat Chat { get; set; } = null!;
    }
}
