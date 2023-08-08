using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
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
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(e=>e.ApplicationUserId == userId,
                includeProperties:"Product")
            };
            return View(ShoppingCartVM);
        }
    }
}
