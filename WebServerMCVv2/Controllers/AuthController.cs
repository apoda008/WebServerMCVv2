using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WebServerMVCv2.Models;
using WebServerMVCv2.Services;

namespace WebServerMCVv2.Controllers
{
    public class AuthController(IAuthService authService) : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserDto model) 
        { 
            //currently works 
            //var account = UserManager.Login(model.Username, model.Password);

            var account = authService.LoginAsync(model);

            if (account != null)
            {
                var identity = new ClaimsIdentity(account.Username, Settings.AuthCookieName);
                var principle = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties 
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                };

                await HttpContext.SignInAsync(Settings.AuthCookieName, principle, props);
            }
            else 
            {
                Console.WriteLine("Failed");
                return View(model);
            }

            return RedirectToAction("Index", "Home");   
        }
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(UserDto request) 
        {
            var user = await authService.RegisterAsync(request);
            if (user is null) 
            {
                return BadRequest("Invalid Username or password");
            }

            return Ok();
        }

        public IActionResult Forbidden() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout() 
        { 
            await HttpContext.SignOutAsync(Settings.AuthCookieName);
            return RedirectToAction("Index", "Home");
        }

    }
}
 