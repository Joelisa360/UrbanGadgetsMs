using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgets.Data;
using UrbanGadgets.Models;

namespace UrbanGadgetsMS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // LIST
        public IActionResult Index(string search, int? categoryId, int page = 1)
        {
            int pageSize = 10;

            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search.ToLower()));
            }

            // CATEGORY
            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // TOTAL ITEMS
            int totalItems = products.Count();

            // PAGINATION
            var pagedProducts = products
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // VIEWBAGS
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            ViewBag.Categories = _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View(pagedProducts);
        }

        // CREATE - GET
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            bool exists = _context.Products.Any(p =>
                p.ProductName.Trim().ToLower() ==
                product.ProductName.Trim().ToLower());

            if (exists)
            {
                TempData["Message"] = "Product already exists";
                TempData["MessageType"] = "warning";

                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["Message"] = "Product added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // EDIT - GET
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                TempData["Message"] = "Please correct the form";
                TempData["MessageType"] = "warning";
                return View(product);
            }

            var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                TempData["Message"] = "Product not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Price = product.Price;
            existingProduct.ReorderLevel = product.ReorderLevel;

            _context.SaveChanges();

            TempData["Message"] = "Product updated successfully";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                TempData["Message"] = "Product not found";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["Message"] = "Product deleted successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        //================== OUT OF STOCK ========================
        public IActionResult OutOfStock()
        {
            var items = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= 0)
                .OrderBy(p => p.ProductName)
                .ToList();

            return View(items);
        }
    }
}