using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanGadgets.Data;
using UrbanGadgets.Models;
using UrbanGadgets.ViewModels;

namespace UrbanGadgets.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginVM model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == model.Username);

        //    if (user == null)
        //    {
        //        ViewBag.Error = "Username or password is incorrect";
        //        return View(model);
        //    }

        //    bool valid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

        //    if (!valid)
        //    {
        //        ViewBag.Error = "Username or password is incorrect";
        //        return View(model);
        //    }

        //    if (!user.IsActive)
        //    {
        //        ViewBag.Error = "Account is disabled";
        //        return View(model);
        //    }

        //    var settings = _context.AppSettings.FirstOrDefault();

        //    // PIN REQUIRED
        //    if (settings?.PinLock == true && settings.RequirePinOnLogin)
        //    {
        //        HttpContext.Session.SetString("PendingUser", user.Username);
        //        return RedirectToAction("EnterPin");
        //    }

        //    // NORMAL LOGIN
        //    await SignUserIn(user);

        //    return RedirectToAction("Index", "Dashboard");
        //}
        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            return Content("LOGIN SUCCESS");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity?.Name;

            var user = _context.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                TempData["Message"] = "User not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Login");
            }

            bool valid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);

            if (!valid)
            {
                ModelState.AddModelError("", "Current password is incorrect");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            TempData["Message"] = "Password changed successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult EnterPin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnterPin(string pin)
        {
            var settings = _context.AppSettings.FirstOrDefault();

            // validate settings
            if (settings == null || string.IsNullOrWhiteSpace(settings.PinCode))
            {
                ViewBag.Error = "No PIN has been configured in Settings.";
                return View();
            }

            // compare clean values
            if (pin?.Trim() != settings.PinCode.Trim())
            {
                ViewBag.Error = "Incorrect PIN";
                return View();
            }

            var username = HttpContext.Session.GetString("PendingUser");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
                return RedirectToAction("Login");

            await SignUserIn(user);

            HttpContext.Session.Remove("PendingUser");

            TempData["Message"] = "Welcome back";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Dashboard");
        }

        private async Task SignUserIn(User user)
        {
            var claims = new List<Claim>
        {
               new Claim(ClaimTypes.Name, user.Username),
               new Claim(ClaimTypes.Role, user.Role),
               new Claim("FullName", user.FullName ?? "")
        };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }
    }
}