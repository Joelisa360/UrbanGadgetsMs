using Microsoft.AspNetCore.Mvc;
//using Org.BouncyCastle.Crypto.Generators;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Cashiers()
        {
            var cashiers = _context.Users
                .Where(x => x.Role == "Cashier")
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
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Message"] = "Cashier added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Cashiers");
        }

        public IActionResult Toggle(int id)
        {
            var user = _context.Users.Find(id);

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
            var user = _context.Users.Find(id);

            if (user == null)
            {
                TempData["Message"] = "Cashier not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Cashiers");
            }

            // default password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            _context.SaveChanges();

            TempData["Message"] =
                $"Password reset for {user.FullName}. Default password is admin123";

            TempData["MessageType"] = "info";

            return RedirectToAction("Cashiers");
        }
    }
}