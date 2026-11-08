using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Options;
using Authentication.Infrastructure.Security;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Authentication.Application.Services
{
    public class AuthServices: IAuthServices
    {
        private readonly UserManager<ApplicationUserEntity> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ITokenServices _tokenServices;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEnumerable<IExternalAuthValidator> _externalAuthValidators;
        public AuthServices(
            UserManager<ApplicationUserEntity> userManager, 
            ITokenServices tokenServices, 
            IOptions<JwtOptions> jwtOptions,
            IRefreshTokenRepository refreshTokenRepository, 
            IEnumerable<IExternalAuthValidator> externalAuthValidators)
        {
            _userManager = userManager;
            _tokenServices = tokenServices;
            _jwtOptions = jwtOptions.Value;
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
                if(!string.IsNullOrWhiteSpace(payload.Email))
                {
                    var ExistingUser = await _userManager.FindByEmailAsync(payload.Email);
                    if(ExistingUser != null)
                    {
                        throw new InvalidOperationException("Email is already registered");
                    }
                }

                user = new ApplicationUserEntity
                {
                    UserName = $"user_{Guid.NewGuid()}:N",
                    Email = payload.EmailVerified ? payload.Email : null,
                    DisplayName = payload.DisplayName,
                    EmailConfirmed = payload.EmailVerified && !string.IsNullOrWhiteSpace(payload.Email)
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Unable to create account from social provider: {errors}");
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
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
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
