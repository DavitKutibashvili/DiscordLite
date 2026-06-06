using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class FriendRequestDTO
    {
        public int FriendshipId { get; set; }
        public string SenderId { get; set; }
        public string SenderUserName { get; set; }
        public string SenderDisplayName { get; set; }
        public bool IsIncoming { get; set; }
    }
}
