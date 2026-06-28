using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using DiscordLite_WEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    public class FriendController : Controller
    {
        private readonly IFriendService _friendService;
        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }
        public async Task<IActionResult> Index()
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
                if (result == null || !result.Success)
                {
                    TempData["error"] = result?.Message.ToString() + "Failed to send friend request";
                }
                else
                {
                    TempData["success"] = $"Friend Request to {userName} sent succesfully";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to send friend request " + ex.ToString();
            }
            return Redirect(Request.Headers.Referer.ToString());
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

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeclineFriendRequest(int id)
        {
            try
            {
                var result = await _friendService.DeclineFriendRequestAsync<ApiResponse<FriendshipDTO>>(id);
                if (result == null || !result.Success)
                    TempData["error"] = "Failed to decline friend request";
                else
                    TempData["success"] = "Friend request declined successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to decline friend request " + ex.ToString();
            }
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }
    }
}
