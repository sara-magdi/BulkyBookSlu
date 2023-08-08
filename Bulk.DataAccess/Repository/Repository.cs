using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulk.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbcontext _db;
        internal DbSet<T> dbset;  
        public Repository(AppDbcontext db)
        {
            _db = db;
            this.dbset = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> Filter, string? includeProperties = null , bool tracked = false  )
        {
            IQueryable<T> query;
            if (tracked)
            {
              query = dbset;
            }
            else
            {
                 query = dbset.AsNoTracking();
            }
            query = query.Where(Filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? Filter,  string? includeProperties = null)
        {
            IQueryable<T> query = dbset;
            if (Filter != null)
            {
                query = query.Where(Filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var includeProperty in  includeProperties
                    .Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries)) 
                { 
                    query = query.Include(includeProperty);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }
    }
}
