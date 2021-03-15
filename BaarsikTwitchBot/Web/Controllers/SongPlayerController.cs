using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace BaarsikTwitchBot.Web.Controllers
{
    public class SongPlayerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}