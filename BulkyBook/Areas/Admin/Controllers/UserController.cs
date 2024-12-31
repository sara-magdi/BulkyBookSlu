using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly AppDbcontext _db;
        public UserController(AppDbcontext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> obj = _db.ApplicationUsers
                .Include(e=>e.Company)
                .ToList();

            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in obj) 
            {

                var roleId = userRole.FirstOrDefault(e => e.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(e => e.Id == roleId).Name;
                if (user.Company == null)
                {
                    user.Company = new() { Name = ""};   
                }
            }
            return Json(new { data = obj });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var obj = _db.ApplicationUsers.FirstOrDefault(e=>e.Id == id);  
            if (obj == null)
            {
                return Json(new { success = true, message = "Erorr While Locking / Unlocking" });
            }
            if(obj.LockoutEnd != null && obj.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unlocked them 
                obj.LockoutEnd = DateTime.Now;
            }
            else
            {
                obj.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successfully" });
        }
        #endregion
    }
}

