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
                TempData["success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id == null ||id== 0)
            {
                return NotFound();
            }
            Category category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]  
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                TempData["success"] = "Category Edit Successfully";

                return RedirectToAction("Index");
            }
            return View(category);
        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int? id)
        {
            Category category = _db.Categories.Find(id);
            if (category == null) 
            {
                return NotFound();
            }
                _db.Categories.Remove(category);
                _db.SaveChanges();
            TempData["success"] = "Category Delete Successfully";

            return RedirectToAction("Index");
        }
    }
}
