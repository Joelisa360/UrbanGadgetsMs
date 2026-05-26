using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UrbanGadgets.Data;
using UrbanGadgets.Models;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using ClosedXML.Excel;

namespace UrbanGadgetsMS.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // 1. SALES LIST (HISTORY)
        // =========================


        public IActionResult Index(
          string? cashier,
          string? receipt,
          DateTime? date)
        {
            var sales = _context.Sales
                .Include(s => s.Product)
                .AsQueryable();

            // FILTER BY DATE
            if (date.HasValue)
            {
                var start = DateTime.SpecifyKind(
                    date.Value.Date,
                    DateTimeKind.Utc);

                var end = start.AddDays(1);

                sales = sales.Where(s =>
                    s.SaleDate >= start &&
                    s.SaleDate < end);
            }

            // FILTER BY CASHIER
            if (!string.IsNullOrWhiteSpace(cashier))
            {
                sales = sales.Where(s =>
                    s.CashierName != null &&
                    s.CashierName.ToLower()
                        .Contains(cashier.ToLower()));
            }

            // FILTER BY RECEIPT
            if (!string.IsNullOrWhiteSpace(receipt))
            {
                sales = sales.Where(s =>
                    s.ReceiptNumber != null &&
                    s.ReceiptNumber.ToLower()
                        .Contains(receipt.ToLower()));
            }

            var model = sales
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            return View(model);
        }

        // =========================
        // 2. SELL PAGE (GET)
        // =========================
        public IActionResult Sell(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                TempData["Message"] = "Product not found";
                return RedirectToAction("Index", "Products");
            }

            ViewBag.Product = product;

            return View(new Sale
            {
                ProductId = product.Id,
                Quantity = 1
            });
        }

        // =========================
        // 3. PROCESS SALE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sell(Sale sale)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == sale.ProductId);

            if (product == null)
            {
                TempData["Message"] = "Product not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Sell");
            }

            if (sale.Quantity <= 0)
            {
                TempData["Message"] = "Quantity must be greater than zero";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Sell");
            }

            if (product.Quantity < sale.Quantity)
            {
                TempData["Message"] = "Not enough stock available";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Sell");
            }

            // =========================
            // CALCULATIONS
            // =========================

            decimal subtotal = product.Price * sale.Quantity;
            decimal discount = sale.Discount;

            decimal cost = product.BuyingPrice * sale.Quantity;

            // Total after discount
            sale.TotalAmount = subtotal - discount;
            if (sale.TotalAmount < 0)
                sale.TotalAmount = 0;

            // Profit after discount
            sale.Profit = (subtotal - discount) - cost;
            if (sale.Profit < 0)
                sale.Profit = 0;
            sale.SaleDate = DateTime.UtcNow;

            // Reduce stock
            product.Quantity -= sale.Quantity;

            // NEW: receipt number
            sale.ReceiptNumber = "INV-" + DateTime.UtcNow.Ticks.ToString().Substring(10);

            // NEW: cashier
            sale.CashierName = User.Identity?.Name ?? "Admin";

            // Save sale
            _context.Sales.Add(sale);
            _context.SaveChanges();

            TempData["Message"] = "Sale completed successfully";
            TempData["MessageType"] = "success";

            // show receipt popup
            TempData["ShowReceiptModal"] = true;
            TempData["SaleId"] = sale.Id;

            // return to products page
            return RedirectToAction("Index", "Products");
        }

        // =========================
        // 4. DELETE SALE (OPTIONAL)
        // =========================
        public IActionResult Delete(int id)
        {
            var sale = _context.Sales.Find(id);

            if (sale == null)
            {
                TempData["Message"] = "Sale not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            _context.Sales.Remove(sale);
            _context.SaveChanges();

            TempData["Message"] = "Sale deleted successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult Receipt(int id)
        {
            var sale = _context.Sales.FirstOrDefault(s => s.Id == id);

            if (sale == null)
                return NotFound();

            var product = _context.Products
                .FirstOrDefault(p => p.Id == sale.ProductId);

            var category = product == null ? null : _context.Categories
                .FirstOrDefault(c => c.Id == product.CategoryId);

            ViewBag.Product = product;
            ViewBag.Category = category;

            return View(sale);
        }

        public IActionResult Leaderboard()
        {
            var data = _context.Sales
                .GroupBy(s => s.CashierName)
                .Select(g => new
                {
                    Cashier = g.Key,
                    TotalSales = g.Sum(x => x.TotalAmount),
                    Transactions = g.Count()
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return View(data);
        }

        public IActionResult ExportExcel()
        {
            var sales = _context.Sales
                .Include(s => s.Product)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sales");

            // Headers
            worksheet.Cell(1, 1).Value = "Product";
            worksheet.Cell(1, 2).Value = "Quantity";
            worksheet.Cell(1, 3).Value = "Total";
            worksheet.Cell(1, 4).Value = "Cashier";
            worksheet.Cell(1, 5).Value = "Receipt No";
            worksheet.Cell(1, 6).Value = "Date";

            int row = 2;

            foreach (var s in sales)
            {
                worksheet.Cell(row, 1).Value = s.Product?.ProductName;
                worksheet.Cell(row, 2).Value = s.Quantity;
                worksheet.Cell(row, 3).Value = s.TotalAmount;
                worksheet.Cell(row, 4).Value = s.CashierName;
                worksheet.Cell(row, 5).Value = s.ReceiptNumber;
                worksheet.Cell(row, 6).Value = s.SaleDate.ToString("dd/MM/yyyy HH:mm");

                row++;
            }

            // formatting (nice touch)
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SalesReport.xlsx"
            );
        }

        
    }
}