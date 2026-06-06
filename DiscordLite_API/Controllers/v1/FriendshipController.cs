using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordLite_API.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendshipsController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipsController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        [HttpPost("request/{username}")]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> SendFriendRequest(string username)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.SendFriendRequestAsync(currentUserId!, username);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/accept")]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AcceptFriendRequest(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.AcceptFriendRequestAsync(id, currentUserId!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/decline")]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeclineFriendRequest(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.DeclineFriendRequestAsync(id, currentUserId!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}/remove")]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFriend(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.RemoveFriendAsync(id, currentUserId!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/block")]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<FriendshipDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BlockUser(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.BlockUserAsync(id, currentUserId!);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FriendDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFriends()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.GetFriendsAsync(currentUserId!);

            if (!result.Success || result.Data == null)
                return StatusCode(result.StatusCode, result);

            var friendDTOs = result.Data.Select(f => new FriendDTO
            {
                FriendshipId = f.Id,
                FriendId = f.RequestedById == currentUserId ? f.ReceivedById : f.RequestedById,
                FriendUserName = f.RequestedById == currentUserId ? f.ReceivedByUsername : f.RequestedByUsername,
                FriendDisplayName = f.RequestedById == currentUserId ? f.ReceivedByDisplayName : f.RequestedByDisplayName
            }).ToList();

            return Ok(ApiResponse<List<FriendDTO>>.Ok(friendDTOs));
        }

        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<List<FriendRequestDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingRequests()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _friendshipService.GetPendingRequestsAsync(currentUserId!);
            if (!result.Success || result.Data == null)
                return StatusCode(result.StatusCode, result);
            var friendRequestDTOs = result.Data?.Select(fr => new FriendRequestDTO
            {
                FriendshipId = fr.Id,
                SenderId = fr.RequestedById,
                SenderUserName = fr.RequestedByUsername,
                SenderDisplayName = fr.RequestedByDisplayName,
                IsIncoming = fr.ReceivedById == currentUserId
            }).ToList();
            return Ok(ApiResponse<List<FriendRequestDTO>>.Ok(friendRequestDTOs));
        }
    }
}
