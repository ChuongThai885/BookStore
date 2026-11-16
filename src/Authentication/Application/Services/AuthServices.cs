using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Options;
using BookStore.Common.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly ICacheService _cacheService;
        public AuthServices(
            UserManager<ApplicationUserEntity> userManager, 
            ITokenServices tokenServices, 
            IOptions<JwtOptions> jwtOptions,
            IRefreshTokenRepository refreshTokenRepository, 
            IEnumerable<IExternalAuthValidator> externalAuthValidators,
            ICacheService cacheService)
        {
            _userManager = userManager;
            _tokenServices = tokenServices;
            _jwtOptions = jwtOptions.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _externalAuthValidators = externalAuthValidators;
            _cacheService = cacheService;
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

            await using var transaction = await _refreshTokenRepository.BeginTransactionAsync(cancellationToken);

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.AbsoluteRefreshTokenExpirationDays);

            var rawRefreshToken = _tokenServices.GenerateRefreshToken();
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);

            var issuedToken = await IssueTokenAsync(
                user,
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                Array.Empty<string>(),
                cancellationToken
            );

            await transaction.CommitAsync( cancellationToken );
            return issuedToken;
        }
        public async Task<AuthTokenDTO> LoginAsync(LoginRequestDTO request, string? ipAddress, CancellationToken cancellationToken)
        { 
            var user = await _userManager.FindByEmailAsync(request.Email) ??
                throw new UnauthorizedAccessException("Invalid email or password");

            if(await _userManager.IsLockedOutAsync(user))
                throw new UnauthorizedAccessException("Account is locked due to multiple failed login attempts. Please try again later.");

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _userManager.AccessFailedAsync(user);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if(user.AccessFailedCount > 0)
                await _userManager.ResetAccessFailedCountAsync(user);

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
                null,
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

            if (string.IsNullOrWhiteSpace(payload.Email) || !payload.EmailVerified)
                throw new UnauthorizedAccessException("Your external account email is not verified. Please verify your email with the provider first.");

            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.AbsoluteRefreshTokenExpirationDays);
            var rawRefreshToken = _tokenServices.GenerateRefreshToken();
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);

            // logged in before
            var user = await _userManager.FindByLoginAsync(payload.Provider, payload.ProviderKey);

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(payload.Email))
                {
                    if (await _userManager.IsLockedOutAsync(user))
                        throw new UnauthorizedAccessException("Account is locked, please try again later.");

                    // log-in succeed
                    return await IssueTokenAsync(
                        user,
                        ipAddress,
                        familyId,
                        absoluteExpiresAt,
                        rawRefreshToken,
                        refreshTokenHash,
                        null,
                        cancellationToken
                    );
                }
            }

            var existingUser = await _userManager.FindByEmailAsync(payload.Email);
            if (existingUser != null)
            {
                if (request.ConfirmLink == null)
                {
                    return new AuthTokenDTO
                    {
                        RequireLinking = true,
                        Email = payload.Email,
                        isExistingEmailConfirmed = existingUser.EmailConfirmed,
                        Message = "An account with this email already exists. Do you want to link your external account to it?"
                    };
                }

                if(request.ConfirmLink == true)
                {
                    if(await _userManager.IsLockedOutAsync(existingUser))
                        throw new UnauthorizedAccessException("Existing account is locked. Please try again later.");

                    if(await _userManager.HasPasswordAsync(existingUser))
                    {
                        if(string.IsNullOrWhiteSpace(request.LinkPassword)) 
                            throw new ArgumentException("Password is required to confirm linking to the existing account.");

                        if(!await _userManager.CheckPasswordAsync(existingUser, request.LinkPassword))
                        {
                            await _userManager.AccessFailedAsync(existingUser);
                            throw new UnauthorizedAccessException("Invalid password for the existing account.");
                        }

                        if (existingUser.AccessFailedCount > 0)
                            await _userManager.ResetAccessFailedCountAsync(existingUser);
                    }

                    await using var linkTransaction = await _refreshTokenRepository.BeginTransactionAsync(cancellationToken);
                    if(!existingUser.EmailConfirmed)
                    {
                        existingUser.EmailConfirmed = true;
                        await _userManager.UpdateAsync(existingUser);
                    }

                    var linkInfo = new UserLoginInfo(payload.Provider, payload.ProviderKey, payload.Provider);
                    var linkResult = await _userManager.AddLoginAsync(existingUser, linkInfo);
                    if (!linkResult.Succeeded)
                        throw new InvalidOperationException("Unable to link external account.");

                    var linkedToken = await IssueTokenAsync(
                        existingUser,
                        ipAddress,
                        familyId,
                        absoluteExpiresAt,
                        rawRefreshToken,
                        refreshTokenHash,
                        null,
                        cancellationToken
                    );

                    await linkTransaction.CommitAsync(cancellationToken);
                    return linkedToken;
                }

                if (!existingUser.EmailConfirmed)
                {
                    await using var reclaimTransaction = await _refreshTokenRepository.BeginTransactionAsync(cancellationToken);

                    var deleteResult = await _userManager.DeleteAsync(existingUser);
                    if (!deleteResult.Succeeded)
                        throw new InvalidOperationException("Unable to remove unverified conflicting account.");

                    return await CreateNewExternalUserAsync(
                        payload,
                        ipAddress,
                        familyId,
                        absoluteExpiresAt,
                        rawRefreshToken,
                        refreshTokenHash,
                        reclaimTransaction,
                        cancellationToken
                    );
                }

                throw new InvalidOperationException("Account linking was declined and this email is already verified by an existing account.");
            }

            // user not existed
            await using var transaction = await _refreshTokenRepository.BeginTransactionAsync(cancellationToken);
            return await CreateNewExternalUserAsync(
                payload,
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                transaction,
                cancellationToken
            );
        } 
        private async Task<AuthTokenDTO> CreateNewExternalUserAsync(
            ExternalUserPayload payload,
            string? ipAddress,
            Guid familyId,
            DateTime absoluteExpiresAt,
            string rawRefreshToken,
            string refreshTokenHash, 
            IDbContextTransaction transaction,
            CancellationToken cancellationToken) 
        {
            var user = new ApplicationUserEntity
            {
                UserName = $"user_{Guid.NewGuid():N}",
                Email = payload.Email,
                DisplayName = payload.DisplayName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to create account from social provider: {errors}");
            }

            var loginInfo = new UserLoginInfo(payload.Provider, payload.ProviderKey, payload.Provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
                throw new InvalidOperationException("Unable to link external account");

            var issuedToken = await IssueTokenAsync(
                user,
                ipAddress,
                familyId,
                absoluteExpiresAt,
                rawRefreshToken,
                refreshTokenHash,
                Array.Empty<string>(),
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
            return issuedToken;
        }
        public async Task<AuthTokenDTO> RefreshTokenAsync(string rawRefreshToken, string? ipAddress, CancellationToken cancellationToken)
        {
            var refreshTokenHash = _tokenServices.HashToken(rawRefreshToken);
            var now = DateTime.UtcNow;

            var storedToken = await _refreshTokenRepository.GetSet()
                .AsNoTracking()
                .Where(t => t.TokenHash == refreshTokenHash)
                .Select(t => new
                {
                    t.FamilyId,
                    t.AbsoluteExpiresAt,
                    t.RevokedAt,
                    t.ExpiresAt,
                    User = t.User
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");

            if (storedToken.User == null)
                throw new UnauthorizedAccessException("User not found");

            if (storedToken.User.LockoutEnabled && storedToken.User.LockoutEnd.HasValue && storedToken.User.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedAccessException("Account is locked.");
            }

            if (storedToken.RevokedAt != null)
            {
                await _refreshTokenRepository.GetSet()
                    .Where(t => t.FamilyId == storedToken.FamilyId && t.RevokedAt == null)
                    .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.RevokedAt, now), cancellationToken);

                throw new UnauthorizedAccessException("Security alert: The session has been compromised. All related sessions have been revoked.");
            }

            if (storedToken.AbsoluteExpiresAt <= now)
                throw new UnauthorizedAccessException("Login session exceeded, please re-login again.");

            if (storedToken.ExpiresAt <= now)
                throw new UnauthorizedAccessException("Refresh token is expired.");

            var roles = await _userManager.GetRolesAsync(storedToken.User);
            var newRawRefreshToken = _tokenServices.GenerateRefreshToken();
            var newRefreshTokenHash = _tokenServices.HashToken(newRawRefreshToken);

            await using var transaction = await _refreshTokenRepository.BeginTransactionAsync(cancellationToken);

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
                await transaction.RollbackAsync(cancellationToken);

                await _refreshTokenRepository.GetSet()
                    .Where(t => t.FamilyId == storedToken.FamilyId && t.RevokedAt == null)
                    .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.RevokedAt, now), cancellationToken);

                throw new UnauthorizedAccessException("Security alert: Concurrent token reuse detected. All related sessions have been revoked.");
            }

            var issuedToken = await IssueTokenAsync(
                storedToken.User,
                ipAddress,
                storedToken.FamilyId,
                storedToken.AbsoluteExpiresAt,
                newRawRefreshToken,
                newRefreshTokenHash,
                roles,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
            return issuedToken;
        }
        private async Task<AuthTokenDTO> IssueTokenAsync(
            ApplicationUserEntity user, 
            string? ipAddress, 
            Guid familyId,
            DateTime absoluteExpiresAt,
            string rawRefreshToken,
            string refreshTokenHash, 
            IList<string>? preloadedRoles,
            CancellationToken cancellationToken)
        {
            var roles = preloadedRoles ?? await _userManager.GetRolesAsync(user);
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

            return new AuthTokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                UserId = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName
            };
        }
        public async Task LogoutAsync(string? rawRefreshToken, string? jti, DateTime? accessTokenExpireAt, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            if(!string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                var tokenHash = _tokenServices.HashToken(rawRefreshToken);

                var targetFamilyIds = _refreshTokenRepository.GetSet(t => t.TokenHash == tokenHash)
                    .Select(t => t.FamilyId);

                await _refreshTokenRepository.GetSet(t => targetFamilyIds.Contains(t.FamilyId) && t.RevokedAt == null)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), cancellationToken);
            }

            if(!string.IsNullOrWhiteSpace(jti))
            {
                var remainingTtl = accessTokenExpireAt.HasValue
                    ? accessTokenExpireAt.Value - now
                    : TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes);

                if(remainingTtl > TimeSpan.Zero)
                {
                    var cacheKey = $"blacklist:jti:{jti}";
                    await _cacheService.SetAsync(cacheKey, true, remainingTtl);
                }
            }
        }
    }
}
