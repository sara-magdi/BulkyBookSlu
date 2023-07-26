using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Data;
using System.Drawing;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> obj = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

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

            if (id == null || id == 0)
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
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }
            

            _unitOfWork.Save();

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string ProductPath = Path.Combine(wwwRootPath, @"Images\Product");

                if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                {

                    var OldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(OldImagePath))
                    {
                        System.IO.File.Delete(OldImagePath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(ProductPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                obj.Product.ImageUrl = @"\Images\Product\" + fileName;




                //if (obj.Product.Id == 0)
                //{
                //    _unitOfWork.Product.Add(obj.Product);
                //}
                //else
                //{
                //    _unitOfWork.Product.Update(obj.Product);
                //}
            }

            _unitOfWork.Save();

            TempData["success"] = "Product Created Successfully";
            return RedirectToAction("Index");
        }
            return View();
    }


    //[HttpGet]
    //public IActionResult Delete(int? id)
    //{
    //    if (id == null || id == 0)
    //    {
    //        return NotFound();
    //    }
    //    Product? product = _unitOfWork.Product.Get(e => e.Id == id);
    //    if (product == null)
    //    {
    //        return NotFound();
    //    }
    //    return View(product);
    //}

    //[HttpPost, ActionName("Delete")]
    //public IActionResult DeleteConfirm(int? id)
    //{
    //    Product product = _unitOfWork.Product.Get(e => e.Id == id);
    //    if (product == null)
    //    {
    //        return NotFound();
    //    }
    //    _unitOfWork.Product.Remove(product);
    //    _unitOfWork.Save();

    //    TempData["success"] = "Product Delete Successfully";

    //    return RedirectToAction("Index");
    //}

    #region API Call
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> obj = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json(new { data = obj });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var objectToDelete = _unitOfWork.Product.Get(e => e.Id == id);
        if (objectToDelete == null)
        {
            return Json(new { Success = false, message = "Error while Deleting" });

        }
        var OldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objectToDelete.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(OldImagePath))
        {
            System.IO.File.Delete(OldImagePath);
        }
        _unitOfWork.Product.Remove(objectToDelete);
        _unitOfWork.Save();
        return Json(new { Success = true, message = "Delete Successfully" });
    }
    #endregion
}
}
