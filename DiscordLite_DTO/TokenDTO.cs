using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class TokenDTO
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
