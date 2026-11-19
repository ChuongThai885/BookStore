using System.Linq.Expressions;

namespace Cart.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetSet(Expression<Func<T, bool>>? predicate = null);
    }
}
