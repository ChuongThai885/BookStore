using Cart.Domain.Interfaces;
using Cart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cart.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _dbcontext;
        protected readonly DbSet<T> _dbSet;
        public BaseRepository(AppDbContext _dbContext)
        {
            this._dbcontext = _dbContext;
            this._dbSet = _dbcontext.Set<T>();
        }
        public IQueryable<T> GetSet(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
            {
                return this._dbSet;
            }
            return this._dbSet.Where(predicate);
        }
    }
}
