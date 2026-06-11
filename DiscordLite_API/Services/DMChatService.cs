using AutoMapper;
using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiscordLite_API.Services
{
    public class DMChatService : IDMChatService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public DMChatService(ApplicationDbContext applicationDbContext, UserManager<User> user, IMapper mapper)
        {
            _db = applicationDbContext;
            _userManager = user;
            _mapper = mapper;
        }
        public async Task<ApiResponse<DMChatDTO>> GetChatAsync(int chatId, string userId)
        {
            var chat = await _db.DirectMessageChats
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null)
            {
                return ApiResponse<DMChatDTO>.NotFound("Chat not found");
            }
            ;
            var user = await _userManager.FindByIdAsync(userId);
            if (chat.User1Id != userId && chat.User2Id != userId)
            {
                return ApiResponse<DMChatDTO>.Forbidden("You do not have permission to access this chat");
            }
            return ApiResponse<DMChatDTO>.Ok(
                _mapper.Map<DMChatDTO>(chat),
                "Chat retrieved successfully"
            );
        }

        public async Task<ApiResponse<List<DMChatDTO>>> GetChatsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<List<DMChatDTO>>.NotFound("User not found");
            }
            var chats = await _db.DirectMessageChats
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.User1)
                .Include(c => c.User2)
                .ToListAsync();

            return ApiResponse<List<DMChatDTO>>.Ok(
                _mapper.Map<List<DMChatDTO>>(chats),
                "Chats retrieved successfully"
            );
        }
    }
}
