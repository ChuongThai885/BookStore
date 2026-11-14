using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Data;

namespace Authentication.Infrastructure.Repository
{
    public class RefreshTokenRepository: BaseRepository<RefreshTokenEntity>,IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context) { }
    }
}
