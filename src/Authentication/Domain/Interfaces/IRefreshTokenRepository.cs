using Authentication.Domain.Entity;
using Authentication.Infrastructure.Repository;

namespace Authentication.Domain.Interfaces
{
    public interface IRefreshTokenRepository: IBaseRepository<RefreshTokenEntity>
    {
    }
}
