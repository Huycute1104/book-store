using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly BStoreDBContext dbContext;
        private readonly DbSet<T> dbSet;

        public GenericRepository(BStoreDBContext dBContext)
        {
            this.dbContext = dBContext;
            this.dbSet = dbContext.Set<T>();


        }
        void IGenericRepository<T>.Add(T item)
        {
            dbSet.Add(item);
            dbContext.SaveChanges();
        }

        void IGenericRepository<T>.Delete(T item)
        {
            dbSet.Remove(item);
            dbContext.SaveChanges();
        }

        public IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includeProperties = "", 
            int? pageIndex = null, 
            int? pageSize = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query.ToList();
        }

        T IGenericRepository<T>.GetById(int id)
        {
            T item = dbSet.Find(id);
            return item;
        }

        void IGenericRepository<T>.Update(T item)
        {
            dbSet.Update(item);
            dbContext.SaveChanges();
        }

        public T GetById(int id, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            var entityType = dbContext.Model.FindEntityType(typeof(T));
            var primaryKey = entityType.FindPrimaryKey().Properties.FirstOrDefault();

            if (primaryKey == null)
            {
                throw new InvalidOperationException($"Entity '{typeof(T).Name}' does not have a primary key defined.");
            }

            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, primaryKey.Name);
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            return query.FirstOrDefault(lambda);
        }
        public T Find(Expression<Func<T, bool>> predicate)
        {
            return dbContext.Set<T>().FirstOrDefault(predicate);
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query.Where(predicate).ToList();
        }
    }
}
