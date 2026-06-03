using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;

namespace UrbanGadgetsMS.Controllers
{
    [Authorize(Roles = "Admin,Cashier,SuperAdmin")]
    public class DashboardController : BaseController
    {
        public DashboardController(AppDbContext context)
            : base(context)
        {
        }

        public IActionResult Index()
        {
            int? businessId = CurrentBusinessId;

            if (businessId == null && !User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var claims = User.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();
            foreach (var c in claims)
            {
                Console.WriteLine(c);
                //Console.WriteLine($"{c.Type} = {claim.Value}");
            }
            // Optional: preload light dashboard stats (safe + useful)
            ViewBag.TotalProducts = _context.Products
                .Count(p => p.BusinessId == businessId);

            ViewBag.TotalSalesToday = _context.Sales
                .Where(s =>
                    s.BusinessId == businessId &&
                    s.SaleDate >= DateTime.UtcNow.Date &&
                    s.SaleDate < DateTime.UtcNow.Date.AddDays(1))
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            ViewBag.LowStockCount = _context.Products
                .Count(p =>
                    p.BusinessId == businessId &&
                    p.Quantity <= p.ReorderLevel);

            ViewBag.TotalExpensesThisMonth = _context.Expenses
                .Where(e =>
                    e.BusinessId == businessId &&
                    e.ExpenseDate >= new DateTime(
                        DateTime.UtcNow.Year,
                        DateTime.UtcNow.Month,
                        1,
                        0, 0, 0,
                        DateTimeKind.Utc))
                .Sum(e => (decimal?)e.Amount) ?? 0;


            return View();
        }
    }
}