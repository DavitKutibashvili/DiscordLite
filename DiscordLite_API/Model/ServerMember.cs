namespace DiscordLite_API.Model
{
    public class ServerMember
    {
        public int Id { get; set; }

        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
