using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServerMCVv2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult UsersOnly() {
            return View();
        }
        [Authorize("admin")]
        public IActionResult AdminOnly() 
        {
            return View();
        }
    }
}
