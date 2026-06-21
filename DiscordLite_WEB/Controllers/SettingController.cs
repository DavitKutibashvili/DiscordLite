using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    public class SettingController : Controller
    {
        private readonly IUserService _userService;
        public SettingController(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _userService.GetUserProfileAsync<ApiResponse<UserDTO>>();
            if (response == null || response.Data == null)
            {
                TempData["error"] = response?.Message ?? "Failed to load user profile";
                return RedirectToAction("Index", "Chat");
            }
            return View(response.Data);
        }
        public async Task<IActionResult> AccountDetails()
        {
            var response = await _userService.GetUserProfileAsync<ApiResponse<UserDTO>>();
            if (response == null || response.Data == null)
            {
                TempData["error"] = response?.Message ?? "Failed to load user profile";
                return RedirectToAction("Index", "Chat");
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
    }
}
