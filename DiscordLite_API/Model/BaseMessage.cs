using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public abstract class BaseMessage
    {
        public int Id { get; set; }
        [Required]
        public string SenderId { get; set; } = null!;
        public User Sender { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
