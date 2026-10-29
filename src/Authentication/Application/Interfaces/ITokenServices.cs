using Authentication.Domain.Entity;

namespace Authentication.Application.Interfaces
{
    public interface ITokenServices
    {
        string GenerateAccessToken(ApplicationUserEntity user, IList<string> roles);
        string GenerateRefreshToken();
        string HashToken(string rawToken);
    }
}
