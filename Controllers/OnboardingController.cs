using Microsoft.AspNetCore.Mvc;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class OnboardingController : BaseController
    {
        public OnboardingController(AppDbContext context)
            : base(context)
        {
        }

        // ================= WELCOME =================
        public IActionResult Welcome()
        {
            var user = GetCurrentUser();

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var businessName = _context.Businesses
                .Where(b => b.Id == CurrentBusinessId)
                .Select(b => b.BusinessName)
                .FirstOrDefault();

            ViewBag.Step = GetOnboardingStep();


            ViewBag.BusinessName = businessName;

            return View();
        }

        // ================= SETUP PAGE =================
        public IActionResult Setup()
        {
            var user = GetCurrentUser();

            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // ================= FINISH SETUP =================
        [HttpPost]
        public IActionResult Finish()
        {
            var user = GetCurrentUser();

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // BUSINESS-LEVEL CHECKS (correct way)
            bool hasCategories = _context.Categories
                .Any(x => x.BusinessId == CurrentBusinessId);

            bool hasRestock = _context.RestockReports
                .Any(x => x.BusinessId == CurrentBusinessId);

            if (!hasCategories || !hasRestock)
            {
                TempData["Error"] =
                    "You must add at least one category and first restock.";

                return RedirectToAction("Welcome");
            }

            // USER UPDATE
            user.IsFirstLogin = false;

            // DISPLAY ADMIN NAV BAR
            var settings = _context.AppSettings
                .FirstOrDefault(x => x.BusinessId == CurrentBusinessId);

            if (settings == null)
            {
                settings = new AppSetting
                {
                    BusinessId = CurrentBusinessId.Value,
                    IsOnboardingComplete = true
                };

                _context.AppSettings.Add(settings);
            }
            else
            {
                settings.IsOnboardingComplete = true;
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }

        public string GetCurrentStep()
        {
            var businessId = CurrentBusinessId;

            if (businessId == null)
                return "complete";

            bool hasCategories = _context.Categories
                .Any(x => x.BusinessId == businessId);

            bool hasCashiers = _context.Users
                .Any(x => x.BusinessId == businessId && x.Role == "Cashier");

            bool hasRestock = _context.RestockReports
                .Any(x => x.BusinessId == businessId);

            if (!hasCategories)
                return "categories";

            if (!hasCashiers)
                return "cashiers";

            if (!hasRestock)
                return "restock";

            return "complete";
        }

        public IActionResult Skip()
        {
            var user = GetCurrentUser();

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // mark onboarding as done
            user.IsFirstLogin = false;

            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}