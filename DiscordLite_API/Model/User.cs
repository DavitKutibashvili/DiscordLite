using Microsoft.AspNetCore.Identity;

namespace DiscordLite_API.Model
{
    public class User : IdentityUser
    {

        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Status { get; set; }
        public DateTime? NameUpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
