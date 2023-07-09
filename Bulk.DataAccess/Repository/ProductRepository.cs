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
            var objForm = _db.Products.FirstOrDefault(e => e.Id == product.Id);   
            if(objForm != null)
            { 
                objForm.Title = product.Title;  
                objForm.Description = product.Description;
                objForm.Price = product.Price;  
                objForm.Author = product.Author;    
                objForm.ListPrice = product.ListPrice;  
                objForm.Price50 = product.Price50;  
                objForm.Price100 = product.Price100;  
                objForm.CategoryId = product.CategoryId;
                objForm.ISBN = product.ISBN;    
                if(product.ImageUrl != null)
                { 
                    objForm.ImageUrl = product.ImageUrl;    
                }
            }
        }

    }
}
