using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;
using UrbanGadgetsMS.ViewModels;

namespace UrbanGadgetsMS.Controllers
{
    public class AuthController : BaseController
    {
        public AuthController(AppDbContext context)
            : base(context)
        {
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(x => x.Username == model.Username);

            if (user == null)
            {
                ViewBag.Error = "Username or password is incorrect";
                return View(model);
            }

            var valid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!valid)
            {
                ViewBag.Error = "Username or password is incorrect";
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Account is disabled";
                return View(model);
            }

            // SUSPENDED BUSINESS LOGIC
            if (user.Role != "SuperAdmin")
            {
                var business = _context.Businesses
                    .FirstOrDefault(x => x.Id == user.BusinessId);

                if (business != null && !business.IsActive)
                {
                    ViewBag.Error = "This business account has been suspended.";
                    return View(model);
                }
            }

            // SUPER ADMIN EXCEPTION
            if (user.Role != "SuperAdmin")
            {
                if (!user.BusinessId.HasValue)
                {
                    ViewBag.Error = "User is not assigned to a business";
                    return View(model);
                }
            }

            var settings = (user.Role == "SuperAdmin")
                ? null
                : _context.AppSettings.FirstOrDefault(x => x.BusinessId == user.BusinessId);

            // PIN LOCK
            if (settings?.PinLock == true && settings.RequirePinOnLogin)
            {
                HttpContext.Session.SetString("PendingUser", user.Username);
                return RedirectToAction("EnterPin");
            }

            await SignUserIn(user, user.BusinessId);

            if (user.Role == "SuperAdmin")
            {
                return RedirectToAction("Index", "SuperAdmin");
            }

            return RedirectToAction("Index", "Dashboard");
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Register()
        {
            return View();
        }

        // ================= REGISTER =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = await _context.PendingRegistrations
                .AnyAsync(x => x.Username == model.Username);

            var userExists = await _context.Users
                .AnyAsync(x => x.Username == model.Username);

            if (exists || userExists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            var request = new PendingRegistration
            {
                Surname = model.Surname,
                OtherName = model.OtherName,
                Contact = model.Contact,
                Email = model.Email,
                BusinessName = model.BusinessName,
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Approved = false,
                RequestedAt = DateTime.UtcNow
            };

            _context.PendingRegistrations.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your account request has been submitted and is awaiting approval.";

            return RedirectToAction(nameof(Login));
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

            Console.WriteLine("USER SIGNED IN");
        }
    }
}