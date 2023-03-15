using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq;
using System.Linq.Expressions;

namespace Pet_Shop_Project
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MyDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(MyDbContext db)
        {
            _db = db;
            //_db.shoppingCart.AsNoTracking()
            //_db.shoppingCart.Include(u => u.animal);
            this.dbSet = db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var inclideProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include("animal");
                }
            }

            return query;
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;

            if (tracked = true)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

                query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var inclideProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include("animal");
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}