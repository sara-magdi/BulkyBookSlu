using Bulk.DataAccess.Repository;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                OrderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(OrderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Update Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employees)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateSatuts(OrderVM.OrderHeader.Id, SD.StatusInProgress);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Update Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employees)]

        public IActionResult ShipOrder()
        {
            var orderHeader =_unitOfWork.OrderHeader.Get(e=>e.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;    
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipping;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDeuDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["Success"] = "Order shipped status Update Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employees)]

        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(e => e.Id == OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var Options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentInteendId,
                };
                var Service = new  RefundService();
                Refund refund = Service.Create(Options);
                _unitOfWork.OrderHeader.UpdateSatuts(orderHeader.Id,SD.StatusCancelled,SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateSatuts(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employees)]

        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader
                .Get(e => e.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail
                .GetAll(e=>e.OrderHeaderId == OrderVM.OrderHeader.Id ,includeProperties:"Product");


            //StripeConfiguration.ApiKey = "sk_test_51NiCEvATWbXqRCyI3FraDy3hPu1SbtXbSDSSFd59zSXOpoCJB3Pqo2lWtSgjtbKTnB48c7lWEhh26FUKs00Woj6X00XeVPmK0F";
            var Domain = "https://localhost:7104/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = Domain + $"admin/order/PaymentConfirmation?OrderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = Domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var SessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "USD",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(SessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int OrderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(e => e.Id == OrderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this order is by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateSatuts(OrderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(OrderHeaderId);
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
