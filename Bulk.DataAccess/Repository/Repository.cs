﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

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

        public T Get(Expression<Func<T, bool>> Filter)
        {
            IQueryable<T> query = dbset;
            query = query.Where(Filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbset;
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
