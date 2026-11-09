using System.Linq.Expressions;
namespace Authentication.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetSet(Expression<Func<T, bool>> predicate = null);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
