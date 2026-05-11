using Microsoft.AspNetCore.Mvc;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Expenses
                .OrderByDescending(x => x.ExpenseDate)
                .ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Expense expense)
        {
            if (!ModelState.IsValid)
                return View(expense);

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            TempData["Message"] = "Expense added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses.Find(id);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();

                TempData["Message"] = "Expense deleted";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetExpenseAlert()
        {
            var month = DateTime.Today.AddMonths(-1);

            var total = _context.Expenses
                .Where(x => x.ExpenseDate >= month)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            var settings = _context.AppSettings.FirstOrDefault();

            return Json(new
            {
                exceeded = total > settings.MonthlyExpenseLimit,
                total
            });
        }
    }
}