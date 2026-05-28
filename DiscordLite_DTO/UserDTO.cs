using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class UserDTO
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public string? DisplayName { get; set; }
        public required string Email { get; set; }
    }
}
