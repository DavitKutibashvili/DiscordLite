using DiscordLite_API.Data;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        public UserService(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }
        public async Task<ApiResponse<object>> UpdateDisplayName(string newDisplayName, string userId)
        {
            if(userId == null)
            {
                return ApiResponse<object>.Unauthorized("User is not authenticated");
            }
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null)
            {
                return ApiResponse<object>.NotFound("User not found");
            }
            if (newDisplayName.Length <=0 || newDisplayName.Length > 32)
            {
                return ApiResponse<object>.BadRequest("Display name must be between 1 to 32 characters");
            }
            user.DisplayName = newDisplayName;
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null!, "Display name updated successfully");
        }
    }
}
