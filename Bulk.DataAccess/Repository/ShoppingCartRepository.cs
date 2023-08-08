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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private  AppDbcontext  _db;
        public ShoppingCartRepository(AppDbcontext db) : base(db)
        {
            _db = db;   
        }

        

        public void Update(ShoppingCart shoppingCart)
        {
            _db.ShoppingCarts.Update(shoppingCart);
        }
    }
}
