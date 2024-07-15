using System.Linq.Expressions;

namespace Repository.GenericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null);

        T Find(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, string includeProperties = "");
        void Add(T item);
        void Update(T item);
        void Delete(T item);
        T GetById(int id);
        T GetById(int id, string includeProperties = "");
        int Count(Expression<Func<T, bool>> filter = null);
    }
}
