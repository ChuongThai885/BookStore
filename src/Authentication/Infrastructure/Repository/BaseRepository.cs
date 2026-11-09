using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Authentication.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public BaseRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }
        public IQueryable<T> GetSet(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this._dbSet;
            }
            return this._dbSet.Where(predicate);
        }
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            EntityEntry<T> result = await this._dbSet.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
