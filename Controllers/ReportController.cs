using Microsoft.AspNetCore.Authorization;
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


        //========================= REPORTS =====================
        public IActionResult Index(DateTime? restockDate, string activeTab = null)
        {
            // DEFAULT TAB
            string tab = string.IsNullOrEmpty(activeTab) ? "daily" : activeTab;

            // UTC SAFE
            var today = DateTime.UtcNow.Date;

            var todayStart = DateTime.SpecifyKind(today, DateTimeKind.Utc);
            var tomorrow = todayStart.AddDays(1);
            var weekStart = todayStart.AddDays(-7);

            // DAILY SALES
            ViewBag.DailySales = _context.Sales
                .Include(x => x.Product)
                .Where(x => x.SaleDate >= todayStart &&
                            x.SaleDate < tomorrow)
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            // WEEKLY SALES
            ViewBag.WeeklySales = _context.Sales
                .Include(x => x.Product)
                .Where(x => x.SaleDate >= weekStart &&
                            x.SaleDate < tomorrow)
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            // RESTOCK FILTER
            if (restockDate.HasValue)
            {
                tab = "restock"; // 🔥 FORCE RESTOCK TAB WHEN FILTERING

                var filterStart = DateTime.SpecifyKind(restockDate.Value.Date, DateTimeKind.Utc);
                var filterEnd = filterStart.AddDays(1);

                ViewBag.Restocks = _context.RestockReports
                    .Where(x => x.ReportDate >= filterStart &&
                                x.ReportDate < filterEnd)
                    .OrderByDescending(x => x.ReportDate)
                    .ToList();

                ViewBag.RestockDate = filterStart.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.Restocks = _context.RestockReports
                    .OrderByDescending(x => x.ReportDate)
                    .Take(5)
                    .ToList();

                ViewBag.RestockDate = "";
            }

            ViewBag.Categories = _context.Categories
                .OrderBy(x => x.CategoryName)
                .ToList();

            // FINAL SINGLE SOURCE OF TRUTH
            ViewBag.ActiveTab = tab;

            return View();
        }


        //======================= CREATE PAGE=================================

        [Authorize(Roles = "Admin")]
        public IActionResult CreateRestock()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View();
        }

        //======================== SAVE ===========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult SaveRestock(RestockReport report)
        {
            report.CreatedBy = User.Identity?.Name ?? "Admin";
            report.ReportDate = DateTime.UtcNow;

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
                        DateAdded = DateTime.UtcNow
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

        //========================= VIEW RESTOCKS =======================
        [Authorize(Roles = "Admin")]
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



        //========================= EXPORT EXCEL ================================
        public IActionResult ExportDailySales()
        {
            var today = DateTime.UtcNow.Date;

            var start = DateTime.SpecifyKind(
                today,
                DateTimeKind.Utc);

            var end = start.AddDays(1);

            var sales = _context.Sales
                .Include(x => x.Product)
                .Where(x => x.SaleDate >= start &&
                            x.SaleDate < end)
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Daily Sales");

            worksheet.Cell(1, 1).Value = "Receipt";
            worksheet.Cell(1, 2).Value = "Product";
            worksheet.Cell(1, 3).Value = "Quantity";
            worksheet.Cell(1, 4).Value = "Total";
            worksheet.Cell(1, 5).Value = "Cashier";
            worksheet.Cell(1, 6).Value = "Date";

            int row = 2;

            foreach (var s in sales)
            {
                worksheet.Cell(row, 1).Value = s.ReceiptNumber;
                worksheet.Cell(row, 2).Value = s.Product?.ProductName;
                worksheet.Cell(row, 3).Value = s.Quantity;
                worksheet.Cell(row, 4).Value = s.TotalAmount;
                worksheet.Cell(row, 5).Value = s.CashierName;

                worksheet.Cell(row, 6).Value =
                    s.SaleDate.ToLocalTime()
                    .ToString("dd MMM yyyy hh:mm tt");

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DailySales-{today:yyyyMMdd}.xlsx");
        }
    }
}