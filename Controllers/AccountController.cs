using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;


namespace UrbanGadgetsMS.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(AppDbContext context)
            : base(context)
        {
        }

        //[HttpPost]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    return RedirectToAction("Login");
        //}

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

            var user = _context.Users
                .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                TempData["Message"] = "User not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Login");
            }

            var valid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);

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

            if (settings == null || string.IsNullOrWhiteSpace(settings.PinCode))
            {
                ViewBag.Error = "No PIN configured.";
                return View();
            }

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

            await SignUserIn(user, user.BusinessId);

            HttpContext.Session.Remove("PendingUser");

            return RedirectToAction("Index", "Dashboard");
        }

        private async Task SignUserIn(User user, int? businessId)
        {
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Name, user.Username),
                 new Claim(ClaimTypes.Role, user.Role),
                 new Claim("FullName", user.FullName ?? "")
            };

            if (user.Role != "SuperAdmin")
            {
                claims.Add(new Claim("BusinessId", businessId?.ToString() ?? ""));
            }
            else
            {
                claims.Add(new Claim("BusinessId", ""));
            }

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}