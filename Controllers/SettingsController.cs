using Microsoft.AspNetCore.Mvc;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

namespace UrbanGadgets.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var settings = _context.AppSettings.FirstOrDefault();

            if (settings == null)
            {
                settings = new AppSetting
                {
                    DarkMode = false,
                    CompactTables = true,
                    ThemeColor = "Blue",
                    LowStockAlerts = true,
                    ExpenseAlerts = true,
                    PopupNotifications = false,
                    PinLock = false,
                    AutoLogout = "15 Minutes",
                    AutoPrintReceipt = false,
                    ConfirmBeforeSale = true,
                    AllowDiscounts = true
                };

                _context.AppSettings.Add(settings);
                _context.SaveChanges();
            }

            return View(settings);
        }

        [HttpPost]
        public IActionResult Save(AppSetting model)
        {
            var settings = _context.AppSettings.FirstOrDefault();

            if (settings == null)
            {
                _context.AppSettings.Add(model);
            }
            else
            {
                // Appearance
                settings.DarkMode = model.DarkMode;
                settings.CompactTables = model.CompactTables;
                settings.ThemeColor = model.ThemeColor;

                // Notifications
                settings.LowStockAlerts = model.LowStockAlerts;
                settings.ExpenseAlerts = model.ExpenseAlerts;
                settings.PopupNotifications = model.PopupNotifications;

                // Security
                settings.PinLock = model.PinLock;
                settings.PinCode = model.PinCode;                 // FIXED
                settings.RequirePinOnLogin = model.RequirePinOnLogin; // FIXED
                settings.AutoLogout = model.AutoLogout;

                // Sales
                settings.AutoPrintReceipt = model.AutoPrintReceipt;
                settings.ConfirmBeforeSale = model.ConfirmBeforeSale;
                settings.AllowDiscounts = model.AllowDiscounts;

                // Expense Limit
                settings.MonthlyExpenseLimit = model.MonthlyExpenseLimit; // FIXED
            }

            _context.SaveChanges();

            TempData["Message"] = "Settings saved successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }
    }
}