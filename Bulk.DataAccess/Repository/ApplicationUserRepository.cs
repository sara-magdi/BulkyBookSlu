using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private  AppDbcontext  _db;
        public ApplicationUserRepository(AppDbcontext db) : base(db)
        {
            _db = db;   
        }

        

        //public void Update(ApplicationUser applicationUser)
        //{
        //    _db.ApplicationUsers.Update(applicationUser);
        //}
    }
}
