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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private  AppDbcontext  _db;
        public CategoryRepository(AppDbcontext db) : base(db)
        {
            _db = db;   
        }

        

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
