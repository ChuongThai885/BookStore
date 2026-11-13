using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Product.Infrastructure.Data;
using Product.Domain.Interfaces;

namespace Product.Infrastructure.Repository
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
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                return await this._dbSet.ToListAsync(cancellationToken);
            }
            return await this._dbSet.Where(predicate).ToListAsync(cancellationToken);
        }
        public async Task<T?> FindForUpdateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await this._dbSet.AsTracking().FirstOrDefaultAsync(predicate, cancellationToken);
        }
        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await this._dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            EntityEntry<T> result = this._dbSet.Add(entity);
            await _dbcontext.SaveChangesAsync(cancellationToken);
            return result.Entity;
        }
        public T Add(T entity) => _dbSet.Add(entity).Entity;
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbcontext.SaveChangesAsync(cancellationToken);
        }
        public void Attach(T entity)
        {
            this._dbSet.Attach(entity);
        }
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            this._dbSet.Remove(entity);
            await _dbcontext.SaveChangesAsync(cancellationToken);
        }
    }
}
