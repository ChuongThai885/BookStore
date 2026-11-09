using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.KeyRotationDays);

            var rawRefreshToken = _tokenServices.GenerateRefreshToken();
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);

            return await IssueTokenAsync(
                user,
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                cancellationToken
            );
        }
        public async Task<AuthTokenDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken)

        { 
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new InvalidOperationException("Invalid email or password");
            }


            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.AbsoluteRefreshTokenExpirationDays);

            var rawRefreshToken = _tokenServices.GenerateRefreshToken();
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);

            return await IssueTokenAsync(
                user,
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                cancellationToken
            );
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
                    UserName = $"user_{Guid.NewGuid():N}",
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

            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.KeyRotationDays);

            var rawRefreshToken = _tokenServices.GenerateRefreshToken();
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);

            return await IssueTokenAsync(
                user, 
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                cancellationToken
            );
        } 
        public async Task<AuthTokenDTO> RefreshTokenAsync(string rawRefreshToken, string? ipAddress, CancellationToken cancellationToken)
        {
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);
            var storedToken = await _refreshTokenRepository.GetSet()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash, cancellationToken);

            if (storedToken == null)
            {
                throw new InvalidOperationException("Invalid or expired refresh token");
            }

            var now = DateTime.UtcNow;
            if (storedToken.AbsoluteExpiresAt <= now)
            {
                throw new UnauthorizedAccessException("Login session exceeded, please re-login again.");
            }

            var newRawRefreshToken = _tokenServices.GenerateRefreshToken();
            var newRefreshTokenHash = _tokenServices.HashToken(newRawRefreshToken);

            var affectedRows = await _refreshTokenRepository.GetSet()
                .Where(t => t.TokenHash == refreshTokenHash
                    && t.RevokedAt == null
                    && t.ExpiresAt > now
                    && t.AbsoluteExpiresAt > now)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(t => t.RevokedAt, now)
                    .SetProperty(t => t.ReplacedByTokenHash, newRefreshTokenHash),
                    cancellationToken
                );

            if (affectedRows == 0)
            {
                if (storedToken.RevokedAt != null)
                {
                    var secondsSinceRevoked = (now - storedToken.RevokedAt.Value).TotalSeconds;
        
                    if (secondsSinceRevoked <= 10)
                    {
                        throw new InvalidOperationException("Refresh token was recently revoked.");
                    }

                    await _refreshTokenRepository.GetSet().Where(t => t.FamilyId == storedToken.FamilyId && t.RevokedAt == null)
                        .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.RevokedAt, now), cancellationToken);

                    throw new UnauthorizedAccessException("Security alert: The session has been compromised. All related sessions have been revoked.");
                }

                throw new UnauthorizedAccessException("Refresh token is expired or invalid.");
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return await IssueTokenAsync(user,
                ipAddress,
                storedToken.FamilyId,
                storedToken.AbsoluteExpiresAt,
                newRawRefreshToken,
                newRefreshTokenHash,
                cancellationToken
            );
        }
        private async Task<AuthTokenDTO> IssueTokenAsync(
            ApplicationUserEntity user, 
            string? ipAddress, 
            Guid familyId,
            DateTime absoluteExpiresAt,
            string rawRefreshToken,
            string refreshTokenHash,
            CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenServices.GenerateAccessToken(user, roles);

            var normalExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

            var effectiveExpiresAt = normalExpiresAt < absoluteExpiresAt ? normalExpiresAt : absoluteExpiresAt;

            var refreshEntity = new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FamilyId = familyId,
                TokenHash = refreshTokenHash,
                ExpiresAt = effectiveExpiresAt,
                AbsoluteExpiresAt = absoluteExpiresAt,
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
