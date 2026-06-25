using DiscordLite_DTO;
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
    }
}
