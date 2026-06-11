using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IMessageService
    {
        Task<ApiResponse<List<MessageDTO>>> GetMessagesAsync(int channelId, string userId, int page, int pageSize);
        Task<ApiResponse<MessageDTO>> SendMessageAsync(int chatId, string senderId, string content);
    }
}
