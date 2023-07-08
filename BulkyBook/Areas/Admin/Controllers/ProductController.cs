using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> obj = _unitOfWork.Product.GetAll().ToList();

            return View(obj);
        }
        public IActionResult Upsert(int? id)
        {
         
            ProductViewModel productViewModel = new()
            {
                CategoryList = _unitOfWork.Category.GetAll()
               .Select(w => new SelectListItem
               {
                   Text = w.Name,
                   Value = w.Id.ToString(),
               }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                //Create
                return View(productViewModel);
            }
            else
            {
                //Update
                productViewModel.Product = _unitOfWork.Product.Get(w => w.Id == id);
                return View(productViewModel);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
       

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? product = _unitOfWork.Product.Get(e => e.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int? id)
        {
            Product product = _unitOfWork.Product.Get(e => e.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            TempData["success"] = "Product Delete Successfully";

            return RedirectToAction("Index");
        }
    }
}
