using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IChannelMessageService
    {
        Task<ApiResponse<List<ChannelMessageDTO>>> GetMessagesAsync(int channelId, string userId, int page, int pageSize);
        Task<ApiResponse<ChannelMessageDTO>> SendMessageAsync(int channelId, string senderId, string content);
    }
}
