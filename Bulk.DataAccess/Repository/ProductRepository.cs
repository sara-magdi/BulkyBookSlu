using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private AppDbcontext _db;
        public ProductRepository(AppDbcontext db) : base(db)
        {
            _db = db;
        }


        public void Update(Product product)
        {
            _db.Products.Update(product);
        }

    }
}
