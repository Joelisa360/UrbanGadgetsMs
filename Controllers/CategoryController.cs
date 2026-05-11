using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanGadgets.Data;
using UrbanGadgets.Models;


namespace UrbanGadgetsMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            ViewData["SortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var categories = _context.Categories.AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.CategoryName.Contains(search));
            }

            // SORT
            categories = sortOrder == "name_desc"
                ? categories.OrderByDescending(c => c.CategoryName)
                : categories.OrderBy(c => c.CategoryName);

            // PAGINATION
            var totalItems = categories.Count();

            var data = categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            bool exists = _context.Categories.Any(c =>
                c.CategoryName.Trim().ToLower() ==
                category.CategoryName.Trim().ToLower());

            if (exists)
            {
                TempData["Message"] = "Category already exists";
                TempData["MessageType"] = "warning";
                return View(category);
            }

            _context.Categories.Add(category);
            _context.SaveChanges();

            TempData["Message"] = "Category added successfully";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            bool exists = _context.Categories.Any(c =>
                c.Id != category.Id &&
                c.CategoryName.Trim().ToLower() ==
                category.CategoryName.Trim().ToLower());

            if (exists)
            {
                TempData["Message"] = "Category name already exists";
                TempData["MessageType"] = "warning";
                return View(category);
            }

            _context.Categories.Update(category);
            _context.SaveChanges();

            TempData["Message"] = "Category updated successfully";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["Message"] = "Category deleted successfully";
            TempData["MessageType"] = "danger";

            return RedirectToAction("Index");
        }

    }
}
