using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public class Server
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [MaxLength(500)]
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        [Required]
        public string OwnerId { get; set; } = null!;
        public User Owner { get; set; } = null!;
        public string? InviteCode { get; set; }
        public DateTime? InviteExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ServerMember> Members { get; set; } = new();
        public List<ServerChannel> Channels { get; set; } = new();
    }
}
