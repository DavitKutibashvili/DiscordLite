using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using DiscordLite_WEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordLite_WEB.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private readonly IFriendService _friendService;
        private readonly IDMChatService _dmChatService;
        public MainController(IFriendService friendService, IDMChatService dMChatService)
        {
            _friendService = friendService;
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
                    OtherDisplayName = chat.Data.User1Id == currentUserId ? chat.Data.User2DisplayName : chat.Data.User1DisplayName
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load chat: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Settings()
        {
            return View();
        }
        public async Task<IActionResult> Friends()
        {
            FriendshipVM friendshipVM = new FriendshipVM();
            try
            {
                var friends = await _friendService.GetFriendsAsync<ApiResponse<List<FriendDTO>>>();
                if (friends == null || !friends.Success)
                    TempData["Error"] = "Failed to load friends";
                if (friends != null && friends.Success && friends.Data != null)
                {
                    friendshipVM.Friends = friends.Data;
                }
                var pendingRequests = await _friendService.GetFriendRequestsAsync<ApiResponse<List<FriendRequestDTO>>>();
                if (pendingRequests != null && pendingRequests.Success && pendingRequests.Data != null)
                {
                    friendshipVM.PendingFriendRequests = pendingRequests.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load friends " + ex.ToString();
            }
            return View(friendshipVM);
        }
        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(string userName)
        {
            try
            {
                var result = await _friendService.SendFriendRequestAsync<ApiResponse<FriendshipDTO>>(userName);
                if(result == null || !result.Success)
                {
                    TempData["error"] = result?.Message.ToString() + "Failed to send friend request";
                }
                else
                {
                    TempData["success"] = $"Friend Request to {userName} sent succesfully";
                }
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Failed to send friend request " + ex.ToString();
            }
            return RedirectToAction(nameof(Friends));
        }
        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest(int id)
        {
            try
            {
                var result = await _friendService.AcceptFriendRequestAsync<ApiResponse<FriendshipDTO>>(id);
                if (result == null || !result.Success)
                    TempData["Error"] = "Failed to accept friend request";
                else
                    TempData["Success"] = "Friend request accepted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to accept friend request " + ex.ToString();
            }

            return RedirectToAction(nameof(Friends));
        }
        [HttpPost]
        public async Task<IActionResult> DeclineFriendRequest(int id)
        {
            try
            {
                var result = await _friendService.DeclineFriendRequestAsync<ApiResponse<FriendshipDTO>>(id);
                if(result == null || !result.Success)
                    TempData["Error"] = "Failed to decline friend request";
                else
                    TempData["Success"] = "Friend request declined successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to decline friend request " + ex.ToString();
            }
            return RedirectToAction(nameof(Friends));
        }
        public async Task<IActionResult> RemoveFriend(int id)
        {
            try
            {
                var result = await _friendService.RemoveFriendAsync<ApiResponse<FriendshipDTO>>(id);
                if (result == null || !result.Success)
                    TempData["Error"] = "Failed to remove friend";
                else
                    TempData["Success"] = "Friend removed successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to remove friend " + ex.ToString();
            }
            return RedirectToAction(nameof(Friends));
        }
        public async Task<IActionResult> Servers()
        {
            return View();
        }
    }
}
