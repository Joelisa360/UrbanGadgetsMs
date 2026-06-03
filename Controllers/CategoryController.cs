using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanGadgetsMS.Data;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : BaseController
    {
        public CategoriesController(AppDbContext context)
            : base(context)
        {
        }

        // ================= LIST =================
        public IActionResult Index(string search, string sortOrder, int page = 1)
        {
            int pageSize = 10;

            ViewData["SortParam"] =
                string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var categories = _context.Categories
                .Where(c => c.BusinessId == CurrentBusinessId)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(c =>
                    c.CategoryName.ToLower().Contains(search.ToLower()));
            }

            // SORT
            categories = sortOrder == "name_desc"
                ? categories.OrderByDescending(c => c.CategoryName)
                : categories.OrderBy(c => c.CategoryName);

            int totalItems = categories.Count();

            var data = categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages =
                (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;

            return View(data);
        }

        // ================= CREATE (GET) =================
        public IActionResult Create()
        {
            return View();
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.BusinessId = CurrentBusinessId;

            bool exists = _context.Categories.Any(c =>
                c.BusinessId == CurrentBusinessId &&
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

        // ================= EDIT (GET) =================
        public IActionResult Edit(int id)
        {
            var category = _context.Categories
                .FirstOrDefault(c =>
                    c.Id == id &&
                    c.BusinessId == CurrentBusinessId);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // ================= EDIT (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            var existing = _context.Categories
                .FirstOrDefault(c =>
                    c.Id == category.Id &&
                    c.BusinessId == CurrentBusinessId);

            if (existing == null)
                return NotFound();

            bool exists = _context.Categories.Any(c =>
                c.Id != category.Id &&
                c.BusinessId == CurrentBusinessId &&
                c.CategoryName.Trim().ToLower() ==
                category.CategoryName.Trim().ToLower());

            if (exists)
            {
                TempData["Message"] = "Category name already exists";
                TempData["MessageType"] = "warning";
                return View(category);
            }

            existing.CategoryName = category.CategoryName;

            _context.SaveChanges();

            TempData["Message"] = "Category updated successfully";
            TempData["MessageType"] = "info";

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            var category = _context.Categories
                .FirstOrDefault(c =>
                    c.Id == id &&
                    c.BusinessId == CurrentBusinessId);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories
                .FirstOrDefault(c =>
                    c.Id == id &&
                    c.BusinessId == CurrentBusinessId);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["Message"] = "Category deleted successfully";
            TempData["MessageType"] = "danger";

            return RedirectToAction(nameof(Index));
        }
    }
}