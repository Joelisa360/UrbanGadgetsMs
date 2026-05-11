using Microsoft.AspNetCore.Mvc;

namespace UrbanGadgetsMS.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
