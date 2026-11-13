using System.Linq.Expressions;

namespace Product.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetSet(Expression<Func<T, bool>>? predicate = null);
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> FindForUpdateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        T Add(T entity);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        void Attach(T entity);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
