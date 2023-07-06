using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbcontext _db;
        public ICategoryRepository Category { get;private set; }   
        public IProductRepository Product { get;private set; }   
        public UnitOfWork(AppDbcontext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);    
            Product = new ProductRepository(_db);    
        }
       // public ICategoryRepository CategoryRepository { get;private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
