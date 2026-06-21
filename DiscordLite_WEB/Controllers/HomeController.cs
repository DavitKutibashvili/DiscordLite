using DiscordLite_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DiscordLite_WEB.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            if(User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }
    }
}
