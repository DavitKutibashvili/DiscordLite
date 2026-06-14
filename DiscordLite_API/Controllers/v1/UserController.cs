using AutoMapper;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordLite_API.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IAvatarService _avatarService;
        public UserController(UserManager<User> userManager, IAvatarService avatarService, IMapper mapper)
        {
            _mapper = mapper;
            _userManager = userManager;
            _avatarService = avatarService;
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(ApiResponse<object>.Unauthorized("User not authenticated"));
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<string>.NotFound("User not found"));
            var userProfile = _mapper.Map<UserDTO>(user);
            return Ok(ApiResponse<UserDTO>.Ok(userProfile, "User profile retrieved successfully"));
        }
        [HttpPatch("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(ApiResponse<object>.Unauthorized("User not authenticated"));
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<string>.NotFound("User not found"));

            var newUrl = await _avatarService.UploadImageAsync(file);
            if (newUrl == null) return BadRequest(ApiResponse<object>.BadRequest("Failed to upload image"));

            if (user.AvatarUrl != null)
                await _avatarService.DeleteImageAsync(user.AvatarUrl);

            user.AvatarUrl = newUrl;
            await _userManager.UpdateAsync(user);

            return Ok(ApiResponse<string>.Ok(newUrl, "Avatar updated successfully"));
        }
        [HttpDelete("avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(ApiResponse<object>.Unauthorized("User not authenticated"));
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<string>.NotFound("User not found"));
            if (user.AvatarUrl != null)
            {
                await _avatarService.DeleteImageAsync(user.AvatarUrl);
                user.AvatarUrl = null;
                await _userManager.UpdateAsync(user);
            }
            return Ok(ApiResponse<object>.Ok(null!, "Avatar deleted successfully"));
        }
    }
}
