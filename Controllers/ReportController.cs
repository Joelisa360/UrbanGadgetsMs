using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        // LIST
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var week = today.AddDays(-7);

            ViewBag.DailySales = _context.Sales
                .Include(x => x.Product)
                .Where(x => x.SaleDate.Date == today)
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            ViewBag.WeeklySales = _context.Sales
                .Include(x => x.Product)
                .Where(x => x.SaleDate >= week)
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            ViewBag.Restocks = _context.RestockReports
                .OrderByDescending(x => x.ReportDate)
                .ToList();

            ViewBag.Categories = _context.Categories
                .OrderBy(x => x.CategoryName)
                .ToList();

            return View();
        }

        // CREATE PAGE
        public IActionResult CreateRestock()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View();
        }


        [HttpPost]
        public IActionResult SaveRestock(RestockReport report)
        {
            report.CreatedBy = User.Identity?.Name ?? "Admin";
            report.ReportDate = DateTime.Now;

            decimal total = 0;

            foreach (var item in report.Items)
            {
                total += item.Quantity * item.BuyingPrice;

                // CATEGORY
                var category = _context.Categories
                    .FirstOrDefault(x => x.CategoryName == item.CategoryName);

                if (category == null)
                {
                    category = new Category
                    {
                        CategoryName = item.CategoryName
                    };

                    _context.Categories.Add(category);
                    _context.SaveChanges();
                }

                // PRODUCT
                var product = _context.Products
                    .FirstOrDefault(x => x.ProductName == item.ProductName);

                if (product == null)
                {
                    product = new Product
                    {
                        ProductName = item.ProductName,
                        CategoryId = category.Id,
                        Quantity = item.Quantity,
                        BuyingPrice = item.BuyingPrice > 0 ? item.BuyingPrice : 0,
                        Price = item.Price > 0 ? item.Price : 0,
                        DateAdded = DateTime.Now
                    };

                    _context.Products.Add(product);
                    _context.SaveChanges();
                }
                else
                {
                    product.Quantity += item.Quantity;

                    //// update buying price only if entered
                    //if (item.BuyingPrice > 0)
                    //    product.BuyingPrice = item.BuyingPrice;

                    //// update selling price only if entered
                    //if (item.Price > 0)
                    //    product.Price = item.Price;

                    // KEEP OLD BUYING PRICE IF EMPTY
                    if (item.BuyingPrice <= 0)
                        item.BuyingPrice = product.BuyingPrice;
                    else
                        product.BuyingPrice = item.BuyingPrice;

                    // KEEP OLD SELLING PRICE IF EMPTY
                    if (item.Price <= 0)
                        item.Price = product.Price;
                    else
                        product.Price = item.Price;

                    _context.Products.Update(product);
                    _context.SaveChanges();
                }

                item.ProductId = product.Id;
            }

            report.TotalAmount = total;

            _context.RestockReports.Add(report);
            _context.SaveChanges();

            TempData["Message"] = "Restock saved successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult ViewReport(int id)
        {
            var report = _context.RestockReports
                .Include(r => r.Items)
                .FirstOrDefault(r => r.Id == id);

            if (report == null)
                return NotFound();

            return View(report);
        }

        [HttpGet]
        public IActionResult GetProductInfo(string name)
        {
            var product = _context.Products
                .Include(x => x.Category)
                .FirstOrDefault(x => x.ProductName == name);

            if (product == null)
                return Json(null);

            return Json(new
            {
                category = product.Category.CategoryName,
                price = product.Price,
                buying = product.BuyingPrice
            });
        }
    }
}