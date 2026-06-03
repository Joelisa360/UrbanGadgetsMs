using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        protected User GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

            return _context.Users
                .FirstOrDefault(u => u.Id.ToString() == userId);
        }

        protected int? CurrentBusinessId
        {
            get
            {
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (role == "SuperAdmin")
                    return null;

                var value = User.FindFirst("BusinessId")?.Value;

                return int.TryParse(value, out var id) ? id : null;
            }
        }

        protected int CurrentUserId
        {
            get
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(userId, out var id) ? id : 0;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var user = GetCurrentUser();

            if (user == null)
                return;

            var controller =
                context.RouteData.Values["controller"]?.ToString();

            // Pages allowed during onboarding
            var allowedControllers = new[]
            {
                "Auth",
                "Onboarding",
                "Categories",
                "Users",
                "Settings",
                "Report"
            };

            if (allowedControllers.Contains(controller))
                return;

            var step = GetOnboardingStep(); Console.WriteLine(
            $"Controller={controller}, Step={GetOnboardingStep()}, FirstLogin={user.IsFirstLogin}");

            if (user.IsFirstLogin && step != "complete")
            {
                context.Result = new RedirectToActionResult(
                    "Welcome",
                    "Onboarding",
                    null
                );

                return;
            }
        }

        protected string GetOnboardingStep()
        {
            var businessId = CurrentBusinessId;

            if (businessId == null)
                return "complete";

            bool hasCategories = _context.Categories.Any(x => x.BusinessId == businessId);
            bool hasCashiers = _context.Users.Any(x => x.BusinessId == businessId && x.Role == "Cashier");
            bool hasRestock = _context.RestockReports.Any(x => x.BusinessId == businessId);

            if (!hasCategories)
                return "categories";

            if (!hasCashiers)
                return "cashiers";

            if (!hasRestock)
                return "restock";

            return "complete";
        }
    }
}