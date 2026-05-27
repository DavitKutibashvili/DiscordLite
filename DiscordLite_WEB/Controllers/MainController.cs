using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Settings()
        {
            return View();
        }
        public IActionResult Friends()
        {
            return View();
        }
        public IActionResult Servers()
        {
            return View();
        }
    }
}
