using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Security;
using Azure.Core;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Application.Services
{
    public class AuthServices: IAuthServices
    {
        private readonly UserManager<ApplicationUserEntity> _userManager;
        private readonly ITokenServices _tokenServices;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEnumerable<IExternalAuthValidator> _externalAuthValidators;
        public AuthServices(
            UserManager<ApplicationUserEntity> userManager, 
            ITokenServices tokenServices, 
            IRefreshTokenRepository refreshTokenRepository, 
            IEnumerable<IExternalAuthValidator> externalAuthValidators)
        {
            _userManager = userManager;
            _tokenServices = tokenServices;
            _refreshTokenRepository = refreshTokenRepository;
            _externalAuthValidators = externalAuthValidators;
        }
        public async Task<AuthTokenDTO> RegisterAsync(RegisterRequestDTO request, string? ipAddress, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already in use");
            }

            var user = new ApplicationUserEntity
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            return await IssueTokenAsync(user, ipAddress, cancellationToken);
        }
        public async Task<AuthTokenDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken)

        { 
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            return await IssueTokenAsync(user, ipAddress, cancellationToken);
        }
        public async Task<AuthTokenDTO> ExternalLoginAsync(ExternalLoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken)
        {
            var validator = _externalAuthValidators.FirstOrDefault(v => 
                v.ProviderName.Equals(request.Provider, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException("Unsupported external provider");

            var payload = await validator.ValidateAsync(request.Token, cancellationToken)
                ?? throw new UnauthorizedAccessException("Invalid external authentication token");

            var user = await _userManager.FindByLoginAsync(payload.Provider, payload.ProviderKey);

            if(user == null)
            {
                if(!string.IsNullOrEmpty(payload.Email))
                {
                    user = await _userManager.FindByEmailAsync(payload.Email);
                    if(user != null && !user.EmailConfirmed)
                    {
                        throw new InvalidOperationException("Email is not confirmed");
                    }
                }
                if (user == null)
                {
                    user = new ApplicationUserEntity
                    {
                        UserName = payload.Email ?? Guid.NewGuid().ToString(),
                        Email = payload.Email,
                        DisplayName = payload.DisplayName,
                        EmailConfirmed = payload.EmailVerified
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        throw new InvalidOperationException("Unable to create account from social provider");
                }

                var loginInfo = new UserLoginInfo(payload.Provider, payload.ProviderKey, payload.Provider);

                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                if(!addLoginResult.Succeeded)
                    throw new InvalidOperationException("Unable to link external account");
            }

            return await IssueTokenAsync(user, ipAddress, cancellationToken);
        } 
        public async Task<AuthTokenDTO> RefreshTokenAsync(string rawRefreshToken, string? ipAddress, CancellationToken cancellationToken)
        {
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.RevokedAt != null)
            {
                throw new InvalidOperationException("Invalid or expired refresh token");
            }
            storedToken.RevokedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return await IssueTokenAsync(user, ipAddress, cancellationToken);
        }
        private async Task<AuthTokenDTO> IssueTokenAsync(ApplicationUserEntity user, string? ipAddress, CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenServices.GenerateAccessToken(user, roles);
            var rawRefreshToken = _tokenServices.GenerateRefreshToken();

            var refreshEntity = new RefreshTokenEntity
            {
                UserId = user.Id,
                TokenHash = _tokenServices.HashToken(rawRefreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            };

            await _refreshTokenRepository.AddAsync(refreshEntity, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return new AuthTokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                UserId = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName
            };
        }
    }
}
