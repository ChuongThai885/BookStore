using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
