using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
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
            return View();
        }
        public async Task<IActionResult> Servers()
        {
            return View();
        }
    }
}
