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
        private readonly IUserService _userService;
        private readonly IServerService _serverService;
        public MainController(IFriendService friendService, IDMChatService dMChatService, IUserService userService, IServerService serverService)
        {
            _friendService = friendService;
            _dmChatService = dMChatService;
            _userService = userService;
            _serverService = serverService;
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
                TempData["error"] = "Failed to load chat: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Settings()
        {
            var response = await _userService.GetUserProfileAsync<ApiResponse<UserDTO>>();
            if (response == null || response.Data == null)
            {
                TempData["error"] = response?.Message ?? "Failed to load user profile";
                return RedirectToAction("Index", "Home");
            }
            return View(response.Data);
        }
        public async Task<IActionResult> AccountDetails()
        {
            var response = await _userService.GetUserProfileAsync<ApiResponse<UserDTO>>();
            if (response == null || response.Data == null)
            {
                TempData["error"] = response?.Message ?? "Failed to load user profile";
                return RedirectToAction("Index", "Home");
            }
            return View(response.Data);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var response = await _userService.UpdateUserAvatarAsync<ApiResponse<string>>(file);
            if (response == null || !response.Success)
                TempData["error"] = response?.Message ?? "Failed to upload avatar";

            return RedirectToAction(nameof(AccountDetails));
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDisplayName(string newDisplayName)
        {
            var response = await _userService.UpdateDisplayNameAsync<ApiResponse<object>>(newDisplayName);
            if (response == null || !response.Success)
                TempData["error"] = response?.Message ?? "Failed to update display name";
            else
                TempData["success"] = "Display name updated successfully";
            return RedirectToAction(nameof(AccountDetails));
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
                TempData["error"] = "Failed to load friends " + ex.ToString();
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
                TempData["error"] = "Failed to send friend request " + ex.ToString();
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
                    TempData["error"] = "Failed to accept friend request";
                else
                    TempData["success"] = "Friend request accepted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to accept friend request " + ex.ToString();
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
                    TempData["error"] = "Failed to decline friend request";
                else
                    TempData["success"] = "Friend request declined successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to decline friend request " + ex.ToString();
            }
            return RedirectToAction(nameof(Friends));
        }
        public async Task<IActionResult> RemoveFriend(int id)
        {
            try
            {
                var result = await _friendService.RemoveFriendAsync<ApiResponse<FriendshipDTO>>(id);
                if (result == null || !result.Success)
                    TempData["error"] = "Failed to remove friend";
                else
                    TempData["success"] = "Friend removed successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to remove friend " + ex.ToString();
            }
            return RedirectToAction(nameof(Friends));
        }
        public async Task<IActionResult> Servers()
        {
            List<ServerDTO> serverList = new();
            try
            {
                var result = await _serverService.GetUserServersAsync<ApiResponse<List<ServerDTO>>>();
                if (result == null || !result.Success)
                    TempData["error"] = "Failed to load servers";

                if(result?.Data != null && result.Success)
                {
                    serverList = result.Data;
                }
            }
            catch(Exception ex)
            {
                TempData["error"] = "Failed to retrieve servers " + ex.ToString();
            }
            return View(serverList);
        }
        [HttpPost]
        public async Task<IActionResult> CreateServer(string name)
        {
            try
            {
                var result = await _serverService.CreateServerAsync<ApiResponse<object>>(name);
                if (result == null || !result.Success)
                    TempData["error"] = result?.Message ?? "Failed to create server.";
                else
                    TempData["success"] = "Server created successfully.";
            }
            catch(Exception ex)
            {
                TempData["error"] = "Failed to create server: " + ex.Message;
            }
            return RedirectToAction(nameof(Servers));
        }
        [HttpPost]
        public async Task<IActionResult> JoinServer(string inviteCode)
        {
            try
            {
                var result = await _serverService.JoinServerAsync<ApiResponse<object>>(inviteCode);
                if (result == null || !result.Success)
                    TempData["error"] = result?.Message ?? "Failed to join server";
                else
                    TempData["success"] = "Successfully joined server";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to create server: " + ex.Message;
            }
            return RedirectToAction(nameof(Servers));
        }
        public async Task<IActionResult> Server(int id)
        {
            var result = await _serverService.GetServerByIdAsync<ApiResponse<ServerDTO>>(id);
            if (result == null || !result.Success)
            {
                TempData["error"] = result?.Message ?? "Failed to load server.";
                return RedirectToAction(nameof(Servers));
            }
            return View(result.Data);
        }
    }
}
