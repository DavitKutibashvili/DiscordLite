using AutoMapper;
using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public MessageService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<MessageDTO>>> GetMessagesAsync(int chatId, string userId, int page, int pageSize)
        {
            var chat = await _db.DirectMessageChats.FindAsync(chatId);
            if (chat == null)
                return ApiResponse<List<MessageDTO>>.NotFound("Chat not found");

            if (chat.User1Id != userId && chat.User2Id != userId)
                return ApiResponse<List<MessageDTO>>.Forbidden("You are not a participant of this chat");

            var messages = await _db.Messages
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return ApiResponse<List<MessageDTO>>.Ok(
                _mapper.Map<List<MessageDTO>>(messages),
                "Messages retrieved successfully"
            );
        }

        public async Task<ApiResponse<MessageDTO>> SendMessageAsync(int chatId, string senderId, string content)
        {
            var user = await _db.Users.FindAsync(senderId);
            if (user == null)
            {
                return ApiResponse<MessageDTO>.NotFound("User not found");
            }
            var chat = await _db.DirectMessageChats.FindAsync(chatId);
            if(chat == null)
            {
                return ApiResponse<MessageDTO>.NotFound("Chat not found");
            }
            if(chat.User1Id != senderId && chat.User2Id != senderId)
            {
                return ApiResponse<MessageDTO>.Forbidden("You are not a participant of this chat");
            }
            var message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow
            };
            await _db.Messages.AddAsync(message);
            message.Sender = user;
            await _db.SaveChangesAsync();
            return ApiResponse<MessageDTO>.CreatedAt(
                _mapper.Map<MessageDTO>(message),
                "Message sent successfully"
            );
        }
    }
}
