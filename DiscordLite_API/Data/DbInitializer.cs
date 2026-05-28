using DiscordLite_API.Model;
using Microsoft.AspNetCore.Identity;

namespace DiscordLite_API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            // Seed roles
            foreach (var role in new[] { "User", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin account from config
            var adminEmail = config["AdminSettings:Email"];
            var adminPassword = config["AdminSettings:Password"];
            var adminUsername = config["AdminSettings:UserName"];

            if (await userManager.FindByEmailAsync(adminEmail!) == null)
            {
                var admin = new User
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    DisplayName = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword!);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
