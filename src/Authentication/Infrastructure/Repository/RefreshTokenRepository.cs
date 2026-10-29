using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repository
{
    public class RefreshTokenRepository: BaseRepository<RefreshTokenEntity>,IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context) { }
        public async Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
        }
    }
}
