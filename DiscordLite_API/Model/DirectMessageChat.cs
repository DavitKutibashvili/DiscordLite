using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public class DirectMessageChat
    {
        public int Id { get; set; }

        [Required]
        public string User1Id { get; set; } = null!;
        public User User1 { get; set; } = null!;

        [Required]
        public string User2Id { get; set; } = null!;
        public User User2 { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
