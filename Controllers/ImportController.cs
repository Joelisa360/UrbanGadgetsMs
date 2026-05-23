//using ClosedXML.Excel;
//using Microsoft.AspNetCore.Mvc;
//using UrbanGadgets.Data;
//using UrbanGadgets.Models;

//public class ImportController : Controller
//{
//    private readonly AppDbContext _context;

//    public ImportController(AppDbContext context)
//    {
//        _context = context;
//    }

//    // Upload Page
//    public IActionResult ImportProducts()
//    {
//        return View();
//    }

//    [HttpPost]
//    public IActionResult ImportProducts(IFormFile file)
//    {
//        if (file == null || file.Length == 0)
//        {
//            TempData["Message"] = "Please select a file";
//            return RedirectToAction("ImportProducts");
//        }

//        using var stream = file.OpenReadStream();
//        using var workbook = new XLWorkbook(stream);

//        var worksheet = workbook.Worksheet(1);
//        var rows = worksheet.RangeUsed().RowsUsed();

//        int imported = 0;

//        foreach (var row in rows.Skip(1)) // skip header
//        {
//            string name = row.Cell(1).GetString();
//            string categoryName = row.Cell(2).GetString();
//            decimal buying = row.Cell(3).GetDecimal();
//            decimal selling = row.Cell(4).GetDecimal();
//            int qty = row.Cell(5).GetValue<int>();

//            if (string.IsNullOrWhiteSpace(name))
//                continue;

//            // CATEGORY
//            var category = _context.Categories
//                .FirstOrDefault(x => x.CategoryName == categoryName);

//            if (category == null)
//            {
//                category = new Category
//                {
//                    CategoryName = categoryName
//                };

//                _context.Categories.Add(category);
//                _context.SaveChanges();
//            }

//            // PRODUCT
//            var product = _context.Products
//                .FirstOrDefault(x => x.ProductName == name);

//            if (product == null)
//            {
//                product = new Product
//                {
//                    ProductName = name,
//                    CategoryId = category.Id,
//                    BuyingPrice = buying,
//                    Price = selling,
//                    Quantity = qty,
//                    DateAdded = DateTime.UtcNow
//                };

//                _context.Products.Add(product);
//            }
//            else
//            {
//                product.Quantity += qty;
//                product.BuyingPrice = buying;
//                product.Price = selling;

//                _context.Products.Update(product);
//            }

//            imported++;
//        }

//        _context.SaveChanges();

//        TempData["Message"] = $"{imported} products imported successfully!";
//        return RedirectToAction("ImportProducts");
//    }
//}