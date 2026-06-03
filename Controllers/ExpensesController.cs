using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class ExpensesController : BaseController
    {
        public ExpensesController(AppDbContext context)
            : base(context)
        {
        }

        // ================= LIST =================
        public IActionResult Index()
        {
            var expenses = _context.Expenses
                .Where(e => e.BusinessId == CurrentBusinessId)
                .OrderByDescending(x => x.ExpenseDate)
                .ToList();

            return View(expenses);
        }

        // ================= CREATE (GET) =================
        public IActionResult Create()
        {
            return View();
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense expense)
        {
            if (!ModelState.IsValid)
                return View(expense);

            // 🔥 IMPORTANT: assign business
            expense.BusinessId = CurrentBusinessId;

            // safety check
            if (expense.BusinessId == 0)
            {
                TempData["Message"] = "Invalid business session";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            expense.ExpenseDate = DateTime.UtcNow;

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            TempData["Message"] = "Expense added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses
                .FirstOrDefault(e =>
                    e.Id == id &&
                    e.BusinessId == CurrentBusinessId);

            if (expense == null)
            {
                TempData["Message"] = "Expense not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            TempData["Message"] = "Expense deleted";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        // ================= ALERT =================
        public IActionResult GetExpenseAlert()
        {
            var monthStart = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1,
                0, 0, 0,
                DateTimeKind.Utc
            );

            var total = _context.Expenses
                .Where(x =>
                    x.BusinessId == CurrentBusinessId &&
                    x.ExpenseDate >= monthStart)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            var settings = _context.AppSettings
                .FirstOrDefault(x => x.BusinessId == CurrentBusinessId);

            decimal limit = settings?.MonthlyExpenseLimit ?? 0;

            return Json(new
            {
                exceeded = limit > 0 && total > limit,
                total
            });
        }
    }
}