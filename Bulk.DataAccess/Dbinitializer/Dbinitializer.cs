using Bulk.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Dbinitializer
{
    public class Dbinitializer : IDbinitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbcontext _db;

        public Dbinitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbcontext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void initialize()
        {
            // migration if they are not applied 
            try {

                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate(); 
                }
            }
            catch (Exception ex)
            {

            }
            //create role if they are not created 
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employees)).GetAwaiter().GetResult();


                // if role are not created ,then we will create admin user as well 

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "rahma@yahoo.com",
                    Email = "rahma@yahoo.com",
                    Name = "rahma",
                    PhoneNumber = "01014349416",
                    StreetAddress = "Street 14",
                    State = "egypt",
                    PostalCode = "12345",
                    City = "Shrqia"

                }, "Admin123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(e => e.Email == "rahma@yahoo.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

            }
            return;
           
        }
    }
}
