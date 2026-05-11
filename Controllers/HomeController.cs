using Microsoft.AspNetCore.Mvc;
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
            var today = DateTime.Today;
            var week = today.AddDays(-7);
            var month = today.AddMonths(-1);

            var dailySales = _context.Sales
                .Where(x => x.SaleDate >= today)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            var weeklySales = _context.Sales
                .Where(x => x.SaleDate >= week)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            var monthlySales = _context.Sales
                .Where(x => x.SaleDate >= month)
                .Sum(x => (decimal?)x.TotalAmount) ?? 0;

            var monthlyExpenses = _context.Expenses
                .Where(x => x.ExpenseDate >= month)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            var grossProfit = _context.Sales
                .Where(x => x.SaleDate >= month)
                .Sum(x => (decimal?)x.Profit) ?? 0;

            var profit = grossProfit - monthlyExpenses;

            ViewBag.DailySales = dailySales;
            ViewBag.WeeklySales = weeklySales;   // FIX
            ViewBag.MonthlySales = monthlySales;
            ViewBag.MonthlyExpenses = monthlyExpenses;
            ViewBag.Profit = profit;

            return View();
        }

        [HttpGet]
        public IActionResult GetDashboardData()
        {
            var today = DateTime.Today;
            var week = today.AddDays(-7);
            var month = today.AddMonths(-1);

            var daily = _context.Sales
                .Where(s => s.SaleDate >= today)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var weekly = _context.Sales
                .Where(s => s.SaleDate >= week)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            var monthly = _context.Sales
                .Where(s => s.SaleDate >= month)
                .Sum(s => (decimal?)s.TotalAmount) ?? 0;

            // ✅ REAL PROFIT CALCULATION
            var monthlyProfit = _context.Sales
                .Where(s => s.SaleDate >= month)
                .Sum(s => (decimal?)s.Profit) ?? 0;

            var expenses = _context.Expenses
                .Where(e => e.ExpenseDate >= month)
                .Sum(e => (decimal?)e.Amount) ?? 0;


            var byCategory = _context.Expenses
                .Where(e => e.ExpenseDate >= month)
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    category = g.Key.ToString(),
                    Total = g.Sum(x => x.Amount)
                })
                .ToList();

            var profit = monthlyProfit - expenses;

            return Json(new
            {
                daily,
                weekly,
                monthly,
                expenses,
                profit,
                byCategory
            });
        }

        public IActionResult GetSalesChart()
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var data = last7Days.Select(day => new
            {
                date = day.ToString("dd MMM"),
                total = _context.Sales
                    .Where(s => s.SaleDate.Date == day)
                    .Sum(s => (decimal?)s.TotalAmount) ?? 0
            });

            return Json(data);
        }

        public IActionResult GetTopProducts()
        {
            var data = _context.Sales
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