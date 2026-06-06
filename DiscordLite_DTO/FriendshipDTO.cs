using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class FriendshipDTO
    {
        public int Id { get; set; }
        public string RequestedById { get; set; } = null!;
        public string RequestedByUsername { get; set; } = null!;
        public string RequestedByDisplayName { get; set; } = null!;
        public string ReceivedById { get; set; } = null!;
        public string ReceivedByUsername { get; set; } = null!;
        public string ReceivedByDisplayName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
