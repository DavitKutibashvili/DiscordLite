using DiscordLite_DTO;
using DiscordLite_Utility;

namespace DiscordLite_API.Services.IServices
{
    public interface IChannelService
    {
        Task<ApiResponse<object>> CreateChannelAsync(int serverId, string name, SD.ChannelType type, string userId);
        Task<ApiResponse<object>> DeleteChannelAsync(int channelId, string userId);
    }
}
