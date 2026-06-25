using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class ChannelMessageDTO
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderUserName { get; set; } = null!;
        public string SenderDisplayName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
