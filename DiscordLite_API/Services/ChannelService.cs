using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using DiscordLite_Utility;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class ChannelService : IChannelService
    {
        private readonly ApplicationDbContext _db;
        public ChannelService(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }
        public async Task<ApiResponse<object>> CreateChannelAsync(int serverId, string name, SD.ChannelType type, string userId)
        {
            if (serverId <= 0)
                return ApiResponse<object>.BadRequest("Invalid server id");
            if (string.IsNullOrWhiteSpace(name))
                return ApiResponse<object>.BadRequest("Invalid name");

            var server = await _db.Servers.FindAsync(serverId);
            if (server == null)
                return ApiResponse<object>.NotFound("Server not found");
            if (server.OwnerId != userId)
                return ApiResponse<object>.Forbidden();

            var position = await _db.ServerChannels
                .Where(c => c.ServerId == serverId)
                .CountAsync() + 1;

            var newChannel = new ServerChannel
            {
                ServerId = serverId,
                Name = name,
                Type = type,
                Position = position + 1
            };

            await _db.ServerChannels.AddAsync(newChannel);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.NoContent("Channel created successfully");
        }

        public async Task<ApiResponse<object>> DeleteChannelAsync(int channelId, string userId)
        {
            var channel = await _db.ServerChannels
                .Include(c => c.Server)
                .FirstOrDefaultAsync(c => c.Id == channelId);

            if (channel == null)
                return ApiResponse<object>.NotFound("Channel not found");
            if (channel.Server.OwnerId != userId)
                return ApiResponse<object>.Forbidden();

            _db.ServerChannels.Remove(channel);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.NoContent("channel deleted successfully");
        }
    }
}
