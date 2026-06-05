using System.ComponentModel.DataAnnotations;

namespace DiscordLite_API.Model
{
    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Declined,
        Blocked
    }

    public class Friendship
    {
        public int Id { get; set; }

        [Required]
        public string RequestedById { get; set; } = null!;
        public User RequestedBy { get; set; } = null!;

        [Required]
        public string ReceivedById { get; set; } = null!;
        public User ReceivedBy { get; set; } = null!;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
