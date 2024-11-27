using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
          
            IEnumerable<Product> ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(ProductList);
        }

        public IActionResult Details(int ProductId)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(e => e.Id == ProductId, includeProperties: "Category"),
                Count = 1,
                ProductId = ProductId
            };

            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
           var ClaimIdentity = (ClaimsIdentity)User.Identity;
            var userId = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(e=>e.ApplicationUserId == userId&&
            e.ProductId == shoppingCart.ProductId);

            if(cart != null)
            {
                cart.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cart);
                _unitOfWork.Save();

            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == userId).Count());   
            }
            TempData["Success"] = "Cart Update Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}