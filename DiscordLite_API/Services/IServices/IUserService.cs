using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IUserService
    {
        Task<ApiResponse<object>> UpdateDisplayName(string newDisplayName, string userId);
    }
}
