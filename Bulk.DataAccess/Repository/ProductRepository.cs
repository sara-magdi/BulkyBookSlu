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
                objForm.ImageUrl = product.ImageUrl = product.ImageUrl ?? "https://www.google.com/url?sa=i&url=https%3A%2F%2Fwww.cairo24.com%2F1917356&psig=AOvVaw1bkeZ3QTgkUT9JAi12mbHm&ust=1727637156052000&source=images&cd=vfe&opi=89978449&ved=0CBQQjRxqFwoTCPiYxaG55ogDFQAAAAAdAAAAABAE";
                if (product.ImageUrl != null)
                { 
                    objForm.ImageUrl = product.ImageUrl;
                    _db.SaveChanges();
                }
            }
        }

    }
}
