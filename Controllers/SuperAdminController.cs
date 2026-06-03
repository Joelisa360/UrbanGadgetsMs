using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : BaseController
    {
        public SuperAdminController(AppDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            ViewBag.Businesses = _context.Businesses.Count();

            ViewBag.Users = _context.Users.Count();

            ViewBag.Revenue = _context.Sales.Sum(x => (decimal?)x.TotalAmount) ?? 0;

            ViewBag.ActiveBusinesses = _context.Businesses.Count(x => x.IsActive);

            ViewBag.SuspendedBusinesses = _context.Businesses.Count(x => !x.IsActive);

            ViewBag.PendingRegistrations = _context.PendingRegistrations.Count(x => !x.Approved);

            return View();
        }

        public IActionResult ManageUsers()
        {
            var users = _context.Users
                .Include(x => x.Business)
                .Where(x => x.Role == "Admin")
                .ToList();

            return View(users);
        }

        public IActionResult PendingRegistrations()
        {
            var pending = _context.PendingRegistrations
                .Where(x => !x.Approved)
                .OrderByDescending(x => x.RequestedAt)
                .ToList();

            return View(pending);
        }

        [HttpGet]
        public IActionResult GetPendingDetails(int id)
        {
            var item = _context.PendingRegistrations
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return NotFound();

            return Json(new
            {
                id = item.Id,
                businessName = item.BusinessName,
                surname = item.Surname,
                otherName = item.OtherName,
                contact = item.Contact,
                email = item.Email,
                username = item.Username,
                requestedAt = item.RequestedAt.ToLocalTime()
                    .ToString("dd MMM yyyy HH:mm")
            });
        }

        [HttpPost]
        public IActionResult Suspend(int id)
        {
            var user = _context.Users
                .Include(x => x.Business)
                .FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                user.IsActive = false;

                if (user.Business != null)
                    user.Business.IsActive = false;

                _context.SaveChanges();
            }

            TempData["Success"] = "User Suspended successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public IActionResult Activate(int id)
        {
            var user = _context.Users
                .Include(x => x.Business)
                .FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                user.IsActive = true;

                if (user.Business != null)
                    user.Business.IsActive = true;

                _context.SaveChanges();
            }

            TempData["Success"] = "User Activated successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public IActionResult ChangeRole(int id, string role)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                user.Role = role;
                _context.SaveChanges();
            }

            TempData["Success"] = "Role updated successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        public IActionResult ViewUsers(int businessId)
        {
            var users = _context.Users
                .Where(u =>
                    u.BusinessId == businessId &&
                    u.Role != "SuperAdmin")
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    username = u.Username,
                    role = u.Role,
                    isActive = u.IsActive
                })
                .ToList();

            return Json(users);
        }

        [HttpPost]
        public IActionResult EnableUser(int id)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            user.IsActive = true;
            _context.SaveChanges();

            TempData["Success"] =
                $"{user.FullName} has been enabled.";

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public IActionResult DisableUser(int id)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            user.IsActive = false;
            _context.SaveChanges();

            TempData["Success"] =
                $"{user.FullName} has been disabled.";

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            string userName = user.FullName;

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Success"] =
                $"{userName} was deleted successfully.";

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user == null)
                return RedirectToAction(nameof(ManageUsers));

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

            _context.SaveChanges();

            TempData["Success"] = "Password reset successfully.";

            return RedirectToAction(nameof(ManageUsers));
        }

        // ================= APPROVE =================
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string role)
        {
            var pending = await _context.PendingRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pending == null)
                return NotFound();

            // check duplicate username in USERS
            var userExists = await _context.Users
                .AnyAsync(x => x.Username == pending.Username);

            if (userExists)
            {
                TempData["Error"] = "Username already exists in system.";
                return RedirectToAction(nameof(PendingRegistrations));
            }

            var business = new Business
            {
                BusinessName = pending.BusinessName,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            var user = new User
            {
                FullName = pending.Surname + " " + pending.OtherName,
                Username = pending.Username,
                PasswordHash = pending.PasswordHash,
                Role = role,
                BusinessId = business.Id,
                IsActive = true,
                IsFirstLogin = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            pending.Approved = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{pending.BusinessName} approved successfully.";

            return RedirectToAction(nameof(PendingRegistrations));
        }

        // ================= REJECT =================
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var pending = _context.PendingRegistrations
                .FirstOrDefault(x => x.Id == id);

            if (pending == null)
                return RedirectToAction(nameof(PendingRegistrations));

            _context.PendingRegistrations.Remove(pending);
            _context.SaveChanges();

            TempData["Success"] =
                $"{pending.BusinessName} registration rejected.";

            return RedirectToAction(nameof(PendingRegistrations));
        }
    }
}