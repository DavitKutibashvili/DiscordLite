using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class ServerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
