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
            _db.Categories.Add(category);    
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
