using DiscordLite_API.Model;
using Microsoft.AspNetCore.Identity;

namespace DiscordLite_API.Validators
{
    public class UsernameValidator<TUser> : IUserValidator<TUser> where TUser : User
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(user.UserName) || user.UserName.Length < 5)
            {
                errors.Add(new IdentityError
                {
                    Code = "UsernameTooShort",
                    Description = "Username must be at least 5 characters."
                });
            }

            return Task.FromResult(errors.Count == 0
                ? IdentityResult.Success
                : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
