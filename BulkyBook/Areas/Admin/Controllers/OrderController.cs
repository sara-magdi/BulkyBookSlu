using Bulk.DataAccess.Repository;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
          
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM  = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(e => e.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(e => e.OrderHeaderId == orderId, includeProperties:"Product"),
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employees)]
        public IActionResult UpdateOrderDetail()
        {
           var OrderHeaderFromDb = _unitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.OrderHeader.Id); 
            OrderHeaderFromDb.Name = OrderVM.OrderHeader.Name;  
            OrderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;    
            OrderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;    
            OrderHeaderFromDb.City= OrderVM.OrderHeader.City;   
            OrderHeaderFromDb.State= OrderVM.OrderHeader.State; 
            OrderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;  

            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                OrderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;    
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                OrderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(OrderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Update Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderHeaderFromDb.Id});
        }
        #region call Api
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employees))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(e=>e.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
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
