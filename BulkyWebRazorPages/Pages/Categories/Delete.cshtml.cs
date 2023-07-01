using BulkyWebRazorPages.Data;
using BulkyWebRazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazorPages.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private AppDbcontext _db;
        public Category Category { get; set; }
        public DeleteModel(AppDbcontext db)
        {
            _db = db;
        }
        public void OnGet(int? id)
        {
            if (id != null && id != 0)
            {
                Category = _db.Categories.Find(id);
            }
        }
        public IActionResult OnPost()
        {
            Category? category = _db.Categories.Find(Category.Id);
            if (category == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "Category Delete Successfully";
            return RedirectToPage("Index");
        }
    }
}
