using BulkyBook.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbcontext _db;
        public CategoryController(AppDbcontext db)
        {
            _db = db;   
        }
        public IActionResult Index()
        {
            List<Category> obj = _db.Categories.ToList();
            return View(obj);
        }
        public IActionResult Create ()
        {
            return View();  
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "the Display Order Can't Exactly Match The Name");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]  
        public IActionResult Edit(Category category)
        { 
            _db.Categories.Update(category);
            _db.SaveChanges();
            return View(category);  
        }
    }
}
