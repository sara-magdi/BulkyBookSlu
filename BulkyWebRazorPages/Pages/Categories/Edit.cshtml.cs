using BulkyWebRazorPages.Data;
using BulkyWebRazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazorPages.Pages.Categories
{
    public class EditModel : PageModel
    {
        private AppDbcontext _db;
        [BindProperty]
        public Category Category { get; set; }
        public EditModel(AppDbcontext db)
        {
            _db = db;
        }
        public void OnGet(int? id )
        {
            if (id != null && id != 0)
            {
                Category = _db.Categories.Find(id);
            }
        }
        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(Category);
                _db.SaveChanges();
                TempData["success"] = "Category Update Successfully";

                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
