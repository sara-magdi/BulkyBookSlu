using BulkyWebRazorPages.Data;
using BulkyWebRazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazorPages.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private AppDbcontext _db;
        [BindProperty]
        public Category Category { get; set; }
        public CreateModel(AppDbcontext db)
        {
            _db = db;
        }
        public void OnGet()
        {
        }
        public IActionResult OnPost() 
        { 
            _db.Categories.Add(Category);
            _db.SaveChanges();
            TempData["success"] = "Category Create Successfully";

            return RedirectToPage("Index");
        }
    }
}
