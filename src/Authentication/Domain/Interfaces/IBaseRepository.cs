using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
namespace Authentication.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetSet(Expression<Func<T, bool>>? predicate = null);
        T Add(T entity);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
