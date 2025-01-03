using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var userId = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            IEnumerable<ProductImage> ProductImage = _unitOfWork.ProductImage.GetAll();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = ProductImage.Where(e => e.ProductId == cart.Product.Id).ToList();
                cart.Price = GetPriceBasedOnQuanity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var userId = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == userId,
                includeProperties: "Product"),

                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(e => e.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuanity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var userId = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart
                .GetAll(e => e.ApplicationUserId == userId,
                includeProperties: "Product,ProductImages");
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(e => e.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuanity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                StripeConfiguration.ApiKey = "sk_test_51NiCEvATWbXqRCyI3FraDy3hPu1SbtXbSDSSFd59zSXOpoCJB3Pqo2lWtSgjtbKTnB48c7lWEhh26FUKs00Woj6X00XeVPmK0F";
                var Domain = "https://localhost:7104/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = Domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = Domain + $"Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
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
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(e => e.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this order is by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateSatuts(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }
                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                    .GetAll(e => e.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();
            
            return View(id);
        }

        public IActionResult Plus(int CartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(e => e.Id == CartId);
            cartFromDB.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDB);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int CartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(e => e.Id == CartId, tracked: true);
            if (cartFromDB.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
              _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCart.Remove(cartFromDB);
            }
            else
            {
                cartFromDB.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int CartId)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.Get(e => e.Id == CartId,tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCart.Remove(cartFromDB);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuanity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }

        }
    }
}
