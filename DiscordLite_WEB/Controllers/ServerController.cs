using DiscordLite_DTO;
using DiscordLite_Utility;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    public class ServerController : Controller
    {
        private readonly IServerService _serverService;
        public ServerController(IServerService serverService)
        {
            _serverService = serverService;
        }
        public async Task<IActionResult> Index()
        {
            List<ServerDTO> serverList = new();
            try
            {
                var result = await _serverService.GetUserServersAsync<ApiResponse<List<ServerDTO>>>();
                if (result == null || !result.Success)
                    TempData["error"] = "Failed to load servers";

                if (result?.Data != null && result.Success)
                {
                    serverList = result.Data;
                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                TempData["error"] = "Failed to create server: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Server(int id, int? channelId)
        {
            var result = await _serverService.GetServerByIdAsync<ApiResponse<ServerDTO>>(id);
            if (result == null || !result.Success)
            {
                TempData["error"] = result?.Message ?? "Failed to load server.";
                return RedirectToAction(nameof(Index));
            }
            var selectedChannel = result.Data.Channels.FirstOrDefault(c => c.Id == channelId)
                      ?? result.Data.Channels.FirstOrDefault();

            ViewBag.SelectedChannelId = selectedChannel?.Id;
            ViewBag.SelectedChannelName = selectedChannel?.Name;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetChannelMessages(int channelId, int page, int pageSize)
        {
            var response = await _serverService.GetChannelMessagesAsync<ApiResponse<List<ChannelMessageDTO>>>(channelId, page, pageSize);
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateChannel(int serverId, string name)
        {
            var dto = new ChannelCreateDTO
            {
                ServerId = serverId,
                Name = name,
                Type = SD.ChannelType.Text
            };
            await _serverService.CreateChannelAsync<ApiResponse<object>>(dto);
            return RedirectToAction("Server", new { id = serverId });
        }
        [HttpGet]
        public async Task<IActionResult> DeleteChannel(int id, int serverId)
        {
            await _serverService.DeleteChannelAsync<ApiResponse<object>>(id);
            return RedirectToAction("Server", new { id = serverId });
        }
        [HttpPost]
        public async Task<IActionResult> LeaveServer(int id)
        {
            var response = await _serverService.LeaveServerAsync<ApiResponse<object>>(id);
            if(!response.Success)
            {
                TempData["error"] = response.Message;
                return Redirect(Request.Headers.Referer.ToString());
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteServer(int id)
        {
            var response = await _serverService.DeleteServerAsync<ApiResponse<object>>(id);
            if (!response.Success)
            {
                TempData["error"] = response.Message;
                return Redirect(Request.Headers.Referer.ToString());
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> GenerateInviteCode(int serverId)
        {
            await _serverService.GenerateInviteCodeAsync<ApiResponse<string>>(serverId);
            return Redirect(Request.Headers.Referer.ToString());
        }
        [HttpPost]
        public async Task<IActionResult> RemoveMember(int serverId, string userId)
        {
            await _serverService.RemoveMemberAsync<ApiResponse<object>>(serverId, userId);
            return Redirect(Request.Headers.Referer.ToString());
        }
    }
}
