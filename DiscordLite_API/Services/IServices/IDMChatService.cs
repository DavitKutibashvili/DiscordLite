using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IDMChatService
    {
        Task<ApiResponse<List<DMChatDTO>>> GetChatsAsync(string userId);
        Task<ApiResponse<DMChatDTO>> GetChatAsync(int chatId, string userId);
    }
}
