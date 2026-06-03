using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DiscordLite_DTO
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email or username is required.")]
        public required string UsernameOrEmail { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
