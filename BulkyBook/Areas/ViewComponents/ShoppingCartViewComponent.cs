using Bulk.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBook.Areas.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var ClaimIdentity = User?.Identity as ClaimsIdentity;
            var claim = ClaimIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim?.Value != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(e => e.ApplicationUserId == claim.Value).Count());
                }
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));

            }
            else
            {
                HttpContext.Session.Clear();  
                return View(0);
            }
        }
    }
}
