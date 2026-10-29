using Authentication.Application.DTOs;

namespace Authentication.Application.Interfaces
{
    public interface IAuthServices
    {
        Task<AuthTokenDTO> RegisterAsync(RegisterRequestDTO reqquest, string? ipAddress, CancellationToken cancellationToken);
        Task<AuthTokenDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken);
        Task<AuthTokenDTO> ExternalLoginAsync(ExternalLoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken);
        Task<AuthTokenDTO> RefreshTokenAsync(string rawRefreshToken, string? ipAddress, CancellationToken cancellationToken);
    }
}
