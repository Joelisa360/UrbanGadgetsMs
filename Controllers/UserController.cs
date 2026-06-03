using Microsoft.AspNetCore.Mvc;
//using Org.BouncyCastle.Crypto.Generators;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class UsersController : BaseController
    {
        public UsersController(AppDbContext context)
        : base(context)
        {
        }
        public IActionResult Cashiers()
        {
            var user = GetCurrentUser();

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var cashiers = _context.Users
                .Where(x =>
                    x.Role == "Cashier" &&
                    x.BusinessId == CurrentBusinessId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(cashiers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            user.Role = "Cashier";
            user.BusinessId = CurrentBusinessId; // 🔥 CRITICAL FIX
            user.IsFirstLogin = false;
            user.IsActive = true;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Message"] = "Cashier added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Cashiers");
        }

        public IActionResult Toggle(int id)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.BusinessId == CurrentBusinessId);

            if (user == null)
                return RedirectToAction("Cashiers");

            user.IsActive = !user.IsActive;

            _context.SaveChanges();

            TempData["Message"] = "Cashier updated";
            TempData["MessageType"] = "info";

            return RedirectToAction("Cashiers");
        }

        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.BusinessId == CurrentBusinessId);

            if (user == null)
            {
                TempData["Message"] = "Cashier not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Cashiers");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            _context.SaveChanges();

            TempData["Message"] =
                $"Password reset for {user.FullName}. Default password is admin123";

            TempData["MessageType"] = "info";

            return RedirectToAction("Cashiers");
        }
    }
}