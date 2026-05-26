using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgets.Data;

namespace UrbanGadgetsMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // PostgreSQL-safe UTC dates
            var today = DateTime.UtcNow.Date;
            var tomorrow = DateTime.SpecifyKind(today.AddDays(1), DateTimeKind.Utc);

            var weekStart = DateTime.SpecifyKind(today.AddDays(-7), DateTimeKind.Utc);
            var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);

            // DAILY SALES
            var dailySales = _context.Sales
                .Where(x => x.SaleDate >= today &&
                            x.SaleDate < tomorrow)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // WEEKLY SALES
            var weeklySales = _context.Sales
                .Where(x => x.SaleDate >= weekStart)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // MONTHLY SALES
            var monthlySales = _context.Sales
                .Where(x => x.SaleDate >= monthStart)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            // MONTHLY EXPENSES
            var monthlyExpenses = _context.Expenses
                .Where(x => x.ExpenseDate >= monthStart)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            // GROSS PROFIT
            var grossProfit = _context.Sales
                .Where(x => x.SaleDate >= monthStart)
                .Sum(x => (decimal?)x.Profit) ?? 0;

            // NET PROFIT
            var profit = grossProfit - monthlyExpenses;

            // TARGET %
            var settings = _context.AppSettings.FirstOrDefault();

            decimal target = settings?.MonthlySalesTarget ?? 5000000;

            var targetPercent = target == 0
                ? 0
                : (monthlySales / target) * 100;

            var targetReached = monthlySales >= target;

            // MOST SOLD ITEM (WEEKLY)
            var topItem = _context.Sales
                .Include(s => s.Product)
                .Where(s => s.SaleDate >= weekStart)
                .GroupBy(s => s.Product.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            // MOST SOLD CATEGORY (WEEKLY)
            var topCategory = _context.Sales
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .Where(s => s.SaleDate >= weekStart)
                .GroupBy(s => s.Product.Category.CategoryName)
                .Select(g => new
                {
                    Name = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            // DAILY DISCOUNTS
            var totalDiscountToday = _context.Sales
                .Where(s => s.SaleDate >= today &&
                            s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.Discount) ?? 0;

            // OUT OF STOCK
            var outOfStock = _context.Products
                .Count(p => p.Quantity <= 0);

            // VIEWBAGS
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

        [HttpGet]
        public IActionResult GetDashboardData()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var weekStart = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var daily = _context.Sales
                .Where(s => s.SaleDate >= today &&
                            s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var weekly = _context.Sales
                .Where(s => s.SaleDate >= weekStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthly = _context.Sales
                .Where(s => s.SaleDate >= monthStart)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthlyProfit = _context.Sales
                .Where(s => s.SaleDate >= monthStart)
                .Sum(s => (decimal?)s.Profit) ?? 0;

            var expenses = _context.Expenses
                .Where(e => e.ExpenseDate >= monthStart)
                .Sum(e => (decimal?)e.Amount) ?? 0;

            var byCategory = _context.Expenses
                .Where(e => e.ExpenseDate >= monthStart)
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    category = g.Key.ToString(),
                    total = g.Sum(x => x.Amount)
                })
                .ToList();

            var profit = monthlyProfit - expenses;

            // TARGET
            var settings = _context.AppSettings.FirstOrDefault();

            decimal target =
                settings?.MonthlySalesTarget ?? 5000000;

            var targetPercent = target == 0
                ? 0
                : (monthly / target) * 100;

            var targetReached = monthly >= target;

            // MOST SOLD ITEM THIS WEEK
            var topItem = _context.Sales
                .Include(s => s.Product)
                .Where(s => s.SaleDate >= weekStart)
                .GroupBy(s => s.Product.ProductName)
                .Select(g => new
                {
                    name = g.Key,
                    qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.qty)
                .FirstOrDefault();

            // TODAY DISCOUNTS
            var totalDiscountToday = _context.Sales
                .Where(s => s.SaleDate >= today &&
                            s.SaleDate < tomorrow)
                .Sum(s => (decimal?)s.Discount) ?? 0;

            return Json(new
            {
                daily,
                weekly,
                monthly,
                expenses,
                profit,
                byCategory,

                target,
                targetPercent,
                targetReached,

                totalDiscountToday,
                topItem
            });
        }

        public IActionResult GetSalesChart()
        {
            var today = DateTime.UtcNow.Date;

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var data = last7Days.Select(day => new
            {
                date = day.ToString("dd MMM"),

                total = _context.Sales
                    .Where(s => s.SaleDate >= day &&
                                s.SaleDate < day.AddDays(1))
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0
            });

            return Json(data);
        }

        public IActionResult GetTopProducts()
        {
            var data = _context.Sales
                .Include(s => s.Product)
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
            var items = _context.Products
                .Where(p => p.Quantity <= p.ReorderLevel)
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