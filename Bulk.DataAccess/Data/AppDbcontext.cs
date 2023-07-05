using Microsoft.EntityFrameworkCore;
using Bulky.Models;

namespace Bulk.DataAccess.Data
{
    public class AppDbcontext : DbContext
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options) 
            :base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(b =>
            {
                b.HasData(new
                {
                    Id = 1,
                    Name = "sara",
                    DisplayOrder = 1,

                });
                b.HasData(new
                {
                    Id = 2,
                    Name = "sara",
                    DisplayOrder = 2,

                });
            });
        }
        public DbSet<Category> Categories { get; set; }
    }
}
