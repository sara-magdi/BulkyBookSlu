using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> obj = _unitOfWork.Company.GetAll().ToList();

            return View(obj);
        }
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company company = _unitOfWork.Company.Get(w => w.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {

                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> obj = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = obj });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var objectToDelete = _unitOfWork.Company.Get(e => e.Id == id);
            if (objectToDelete == null)
            {
                return Json(new { Success = false, message = "Error while Deleting" });

            }
         
            _unitOfWork.Company.Remove(objectToDelete);
            _unitOfWork.Save();
            return Json(new { Success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}

