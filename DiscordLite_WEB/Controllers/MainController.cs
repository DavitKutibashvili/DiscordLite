using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using DiscordLite_WEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private readonly IFriendService _friendService;
        public MainController(IFriendService friendService)
        {
            _friendService = friendService;
        }
        public async Task<IActionResult> Index()
        {
            return View();
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
