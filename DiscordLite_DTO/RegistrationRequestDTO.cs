using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DiscordLite_DTO
{
    public class RegistrationRequestDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Username must be at least 5 characters.")]
        [MaxLength(32, ErrorMessage = "Username cannot exceed 32 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_\-.]+$",
            ErrorMessage = "Username can only contain letters, numbers, underscores, hyphens, and dots.")]
        public required string UserName { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "Display name must be at least 1 character.")]
        [MaxLength(32, ErrorMessage = "Display name cannot exceed 32 characters.")]
        public required string DisplayName { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public required string Password { get; set; }
    }
}
