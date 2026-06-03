using Microsoft.AspNetCore.Mvc;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class SettingsController : BaseController
    {
        public SettingsController(AppDbContext context)
        : base(context)
        {
        }

        public IActionResult Index()
        {
            var settings = _context.AppSettings
                .FirstOrDefault(x => x.BusinessId == CurrentBusinessId);

            if (settings == null)
            {
                settings = new AppSetting
                {
                    BusinessId = CurrentBusinessId, // 🔥 CRITICAL FIX

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
            var settings = _context.AppSettings
                .FirstOrDefault(x => x.BusinessId == CurrentBusinessId);

            if (settings == null)
            {
                model.BusinessId = CurrentBusinessId; // 🔥 CRITICAL FIX
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
                settings.PinCode = model.PinCode;
                settings.RequirePinOnLogin = model.RequirePinOnLogin;
                settings.AutoLogout = model.AutoLogout;

                // Sales
                settings.AutoPrintReceipt = model.AutoPrintReceipt;
                settings.ConfirmBeforeSale = model.ConfirmBeforeSale;
                settings.AllowDiscounts = model.AllowDiscounts;

                // Limits
                settings.MonthlyExpenseLimit = model.MonthlyExpenseLimit;
                settings.MonthlySalesTarget = model.MonthlySalesTarget;
            }

            _context.SaveChanges();

            TempData["Message"] = "Settings saved successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }
    }
}