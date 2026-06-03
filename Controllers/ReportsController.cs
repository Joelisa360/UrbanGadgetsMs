using Microsoft.AspNetCore.Mvc;
using UrbanGadgetsMS.Data;

namespace UrbanGadgetsMS.Controllers
{
    public class ReportsController : BaseController
    {
        public ReportsController(AppDbContext context)
        : base(context)
        {
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
