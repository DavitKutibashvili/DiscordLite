using AutoMapper;
using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace DiscordLite_API.Services
{
    public class ChannelMessageService : IChannelMessageService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public ChannelMessageService(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _db = applicationDbContext;
            _mapper = mapper;
        }
        public async Task<ApiResponse<List<ChannelMessageDTO>>> GetMessagesAsync(int channelId, string userId, int page, int pageSize)
        {
            if (channelId <= 0)
                return ApiResponse<List<ChannelMessageDTO>>.BadRequest("Invalid channel id");
            var channel = await _db.ServerChannels.FindAsync(channelId);
            if (channel == null)
                return ApiResponse<List<ChannelMessageDTO>>.BadRequest("Channel not found");
            bool isMember = await _db.ServerMembers.AnyAsync(m => m.ServerId == channel.ServerId && m.UserId == userId);
            if (!isMember)
                return ApiResponse<List<ChannelMessageDTO>>.Forbidden();
            var messages = await _db.ChannelMessages
                .Where(m => m.ChannelId == channelId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ApiResponse<List<ChannelMessageDTO>>.Ok(_mapper.Map<List<ChannelMessageDTO>>(messages), "Messages retrieved successfully");
        }

        public async Task<ApiResponse<ChannelMessageDTO>> SendMessageAsync(int channelId, string senderId, string content)
        {
            var user = await _db.Users.FindAsync(senderId);
            if (user == null)
            {
                return ApiResponse<ChannelMessageDTO>.NotFound("User not found");
            }
            if (string.IsNullOrWhiteSpace(content))
                return ApiResponse<ChannelMessageDTO>.BadRequest("Message cannot be empty");
            if (content.Length > 2000)
                return ApiResponse<ChannelMessageDTO>.BadRequest("Message cannot exceed 2000 characters");
            var channel = await _db.ServerChannels.FindAsync(channelId);
            if (channel == null)
            {
                return ApiResponse<ChannelMessageDTO>.NotFound("Channel not found");
            }
            bool ismember = await _db.ServerMembers.AnyAsync(m => m.UserId == senderId && m.ServerId == channel.ServerId);
            if (!ismember)
            {
                return ApiResponse<ChannelMessageDTO>.Forbidden();
            }
            var message = new ChannelMessage
            {
                ChannelId = channelId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow
            };
            await _db.ChannelMessages.AddAsync(message);
            message.Sender = user;
            await _db.SaveChangesAsync();
            return ApiResponse<ChannelMessageDTO>.CreatedAt(_mapper.Map<ChannelMessageDTO>(message), "Message sent successfully");
        }
    }
}
