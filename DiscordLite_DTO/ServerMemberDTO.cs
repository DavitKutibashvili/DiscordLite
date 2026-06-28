using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class ServerMemberDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAlreadyFriend { get; set; }
    }
}
