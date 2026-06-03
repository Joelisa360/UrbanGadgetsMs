using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;

namespace UrbanGadgetsMS.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(AppDbContext context) : base(context) { }

        public IActionResult Index()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var weekStart = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            int? businessId = CurrentBusinessId;
            bool isSuperAdmin = businessId == null;

            // ================= DAILY SALES =================
            var dailySales = _context.Sales
                .Where(x =>
                    (isSuperAdmin || x.BusinessId == businessId) &&
                    x.SaleDate >= today &&
                    x.SaleDate < tomorrow)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // ================= WEEKLY SALES =================
            var weeklySales = _context.Sales
                .Where(x =>
                    (isSuperAdmin || x.BusinessId == businessId) &&
                    x.SaleDate >= weekStart)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // ================= MONTHLY SALES =================
            var monthlySales = _context.Sales
                .Where(x =>
                    (isSuperAdmin || x.BusinessId == businessId) &&
                    x.SaleDate >= monthStart)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // ================= EXPENSES =================
            var monthlyExpenses = _context.Expenses
                .Where(x =>
                    (isSuperAdmin || x.BusinessId == businessId) &&
                    x.ExpenseDate >= monthStart)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            // ================= PROFIT =================
            var grossProfit = _context.Sales
                .Where(x =>
                    (isSuperAdmin || x.BusinessId == businessId) &&
                    x.SaleDate >= monthStart)
                .Sum(x => (decimal?)x.Profit) ?? 0;

            var profit = grossProfit - monthlyExpenses;

            // ================= SETTINGS =================
            var settings = isSuperAdmin
                ? _context.AppSettings.FirstOrDefault()
                : _context.AppSettings.FirstOrDefault(x => x.BusinessId == businessId.Value);

            decimal target = settings?.MonthlySalesTarget ?? 5_000_000;
            var targetPercent = target == 0 ? 0 : (monthlySales / target) * 100;
            var targetReached = monthlySales >= target;

            // ================= TOP ITEM =================
            var topItem = _context.Sales
                .Include(s => s.Product)
                .Where(s =>
                    (isSuperAdmin || s.BusinessId == businessId) &&
                    s.SaleDate >= weekStart)
                .GroupBy(s => s.Product.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            // ================= TOP CATEGORY =================
            var topCategory = _context.Sales
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .Where(s =>
                    (isSuperAdmin || s.BusinessId == businessId) &&
                    s.SaleDate >= weekStart)
                .GroupBy(s => s.Product.Category.CategoryName)
                .Select(g => new
                {
                    Name = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            // ================= DISCOUNTS =================
            var totalDiscountToday = _context.Sales
                .Where(s =>
                    (isSuperAdmin || s.BusinessId == businessId) &&
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.Discount) ?? 0;

            // ================= OUT OF STOCK =================
            var outOfStock = _context.Products
                .Count(p =>
                    (isSuperAdmin || p.BusinessId == businessId) &&
                    p.Quantity <= 0);

            ViewBag.DailySales = dailySales;
            ViewBag.WeeklySales = weeklySales;
            ViewBag.MonthlySales = monthlySales;
            ViewBag.MonthlyExpenses = monthlyExpenses;
            ViewBag.Profit = profit;

            ViewBag.Target = target;
            ViewBag.TargetPercent = targetPercent;
            ViewBag.TargetReached = targetReached;

            ViewBag.TopItem = topItem;
            ViewBag.TopCategory = topCategory;
            ViewBag.TotalDiscountToday = totalDiscountToday;
            ViewBag.OutOfStock = outOfStock;

            return View();
        }

        // ================= AJAX =================
        [HttpGet]
        public IActionResult GetDashboardData()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            int? businessId = CurrentBusinessId;
            bool isSuperAdmin = businessId == null;

            var daily = _context.Sales
                .Where(s => (isSuperAdmin || s.BusinessId == businessId)
                    && s.SaleDate >= today && s.SaleDate < today.AddDays(1))
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var weekly = _context.Sales
                .Where(s => (isSuperAdmin || s.BusinessId == businessId)
                    && s.SaleDate >= weekStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthly = _context.Sales
                .Where(s => (isSuperAdmin || s.BusinessId == businessId)
                    && s.SaleDate >= monthStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var expenses = _context.Expenses
                .Where(e => (isSuperAdmin || e.BusinessId == businessId)
                    && e.ExpenseDate >= monthStart)
                .Sum(e => (decimal?)e.Amount) ?? 0;

            var profit = _context.Sales
                .Where(s => (isSuperAdmin || s.BusinessId == businessId)
                    && s.SaleDate >= monthStart)
                .Sum(s => (decimal?)s.Profit) ?? 0
                - expenses;

            var settings = isSuperAdmin
                ? _context.AppSettings.FirstOrDefault()
                : _context.AppSettings.FirstOrDefault(x => x.BusinessId == businessId.Value);

            decimal target = settings?.MonthlySalesTarget ?? 5_000_000;

            var targetPercent = target == 0 ? 0 : (monthly / target) * 100;

            return Json(new
            {
                daily,
                weekly,
                monthly,
                expenses,
                profit,
                target,
                targetPercent
            });
        }

        // ================= OTHER METHODS =================

        public IActionResult GetSalesChart()
        {
            var today = DateTime.UtcNow.Date;
            int? businessId = CurrentBusinessId;
            bool isSuperAdmin = businessId == null;

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .OrderBy(d => d);

            var data = last7Days.Select(day => new
            {
                date = day.ToString("dd MMM"),
                total = _context.Sales
                    .Where(s => (isSuperAdmin || s.BusinessId == businessId)
                        && s.SaleDate >= day
                        && s.SaleDate < day.AddDays(1))
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0
            });

            return Json(data);
        }

        public IActionResult GetTopProducts()
        {
            int? businessId = CurrentBusinessId;
            bool isSuperAdmin = businessId == null;

            var data = _context.Sales
                .Include(s => s.Product)
                .Where(s => isSuperAdmin || s.BusinessId == businessId)
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    product = g.First().Product.ProductName,
                    total = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToList();

            return Json(data);
        }

        public IActionResult GetLowStock()
        {
            int? businessId = CurrentBusinessId;
            bool isSuperAdmin = businessId == null;

            var items = _context.Products
                .Where(p => (isSuperAdmin || p.BusinessId == businessId)
                    && p.Quantity <= p.ReorderLevel)
                .Select(p => new
                {
                    name = p.ProductName,
                    qty = p.Quantity
                })
                .ToList();

            return Json(items);
        }
    }
}