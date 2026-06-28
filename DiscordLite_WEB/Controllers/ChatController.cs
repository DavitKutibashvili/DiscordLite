using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using DiscordLite_WEB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordLite_WEB.Controllers
{
    public class ChatController : Controller
    {
        private readonly IDMChatService _dmChatService;
        public ChatController(IDMChatService dMChatService)
        {
            _dmChatService = dMChatService;
        }
        public async Task<IActionResult> Index()
        {
            DMChatsVM dmChatsVM = new DMChatsVM();
            try
            {
                dmChatsVM.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var chatsList = await _dmChatService.GetChatsAsync<ApiResponse<List<DMChatDTO>>>();
                if (chatsList == null || !chatsList.Success)
                {
                    TempData["Error"] = "Failed to load chats";
                }
                if (chatsList != null && chatsList.Success && chatsList.Data != null)
                {
                    dmChatsVM.DMChats = chatsList.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load chats " + ex.ToString();
            }

            return View(dmChatsVM);
        }
        public async Task<IActionResult> OpenChat(int chatId)
        {
            try
            {
                var chat = await _dmChatService.GetChatAsync<ApiResponse<DMChatDTO>>(chatId);

                if (chat == null)
                {
                    TempData["Error"] = "Failed to load chat";
                    return RedirectToAction(nameof(Index));
                }

                if (chat.StatusCode == 403)
                {
                    TempData["Error"] = "You do not have permission to access this chat";
                    return RedirectToAction(nameof(Index));
                }

                if (!chat.Success || chat.Data == null)
                {
                    TempData["Error"] = "Failed to load chat";
                    return RedirectToAction(nameof(Index));
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vm = new OpenDMChatVM
                {
                    ChatId = chat.Data.ChatId,
                    OtherUserId = chat.Data.User1Id == currentUserId ? chat.Data.User2Id : chat.Data.User1Id,
                    OtherUserName = chat.Data.User1Id == currentUserId ? chat.Data.User2UserName : chat.Data.User1UserName,
                    OtherDisplayName = chat.Data.User1Id == currentUserId ? chat.Data.User2DisplayName : chat.Data.User1DisplayName,
                    OtherAvatarUrl = chat.Data.User1Id == currentUserId ? chat.Data.User2AvatarUrl : chat.Data.User1AvatarUrl
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to load chat: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> OpenOrCreateChat(string otherUserId)
        {
            try
            {
                var response = await _dmChatService.GetChatsAsync<ApiResponse<List<DMChatDTO>>>();
                if (response == null || !response.Success || response.Data == null)
                {
                    TempData["error"] = "Failed to open chat";
                    return RedirectToAction("Index", "Friend");
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var chat = response.Data.FirstOrDefault(c =>
                    (c.User1Id == currentUserId && c.User2Id == otherUserId) ||
                    (c.User2Id == currentUserId && c.User1Id == otherUserId));

                if (chat == null)
                {
                    TempData["error"] = "Chat not found";
                    return RedirectToAction("Index", "Friend");
                }

                return RedirectToAction(nameof(OpenChat), new { chatId = chat.ChatId });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to open chat: " + ex.Message;
                return RedirectToAction("Index", "Friend");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMessages(int chatId, int page, int pageSize)
        {
            var response = await _dmChatService.GetChatMessagesAsync<ApiResponse<List<MessageDTO>>>(chatId, page, pageSize);
            return Ok(response);
        }
    }
}
