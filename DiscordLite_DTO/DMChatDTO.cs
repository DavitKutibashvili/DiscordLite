using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class DMChatDTO
    {
        public int ChatId { get; set; }
        public string User1Id { get; set; } = null!;
        public string User2Id { get; set; } = null!;
        public string User1UserName { get; set; } = null!;
        public string User1DisplayName { get; set; } = null!;
        public string User2UserName { get; set; } = null!;
        public string User2DisplayName { get; set; } = null!;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
