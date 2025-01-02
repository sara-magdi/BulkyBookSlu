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
                productViewModel.Product = _unitOfWork.Product.Get(w => w.Id == id ,includeProperties:"ProductImages");
                return View(productViewModel);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                // Add or update the product
                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }

                _unitOfWork.Save();

                // Save images if uploaded
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null && files.Count > 0)
                {
                    string productPath = Path.Combine("Images", "Products", "Product-" + obj.Product.Id);
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    // Ensure the directory exists
                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + ".jpg";
                       // string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(finalPath, fileName);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        // Generate the correct ImageUrl for the web
                        string imageUrl = "/" + Path.Combine(productPath, fileName).Replace("\\", "/");

                        ProductImage productImage = new()
                        {
                            ImageUrl = imageUrl,
                            ProductId = obj.Product.Id,
                        };

                        if (obj.Product.ProductImages == null)
                            obj.Product.ProductImages = new List<ProductImage>();

                        obj.Product.ProductImages.Add(productImage);
                    }

                    _unitOfWork.Product.Update(obj.Product);
                    _unitOfWork.Save();
                }

                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

                return View(obj);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
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
            //var OldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objectToDelete.ImageUrl.TrimStart('\\'));
            //if (System.IO.File.Exists(OldImagePath))
            //{
            //    System.IO.File.Delete(OldImagePath);
            //}
            _unitOfWork.Product.Remove(objectToDelete);
            _unitOfWork.Save();
            return Json(new { Success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
