using Bulk.DataAccess.Repository;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region call Api
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(e=>e.PaymentStatus == SD.PaymentStatusPending); 
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(e => e.OrderStatus == SD.StatusInProgress);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(e => e.OrderStatus == SD.StatusShipping);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(e => e.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = objOrderHeaders });
        }
        #endregion

    }
}
