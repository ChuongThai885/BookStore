using Authentication.Domain.Options;
using Authentication.Infrastructure.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.BackgroundJobs
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;
        private readonly JwtOptions _jwtOptions;

        public TokenCleanupBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<TokenCleanupBackgroundService> logger, IOptions<JwtOptions> jwtOptions)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(_jwtOptions.CleanTokenDurationHours));

            do
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var cutoffDate = DateTime.UtcNow.AddDays(- _jwtOptions.RefreshTokenExpirationDays);

                    var deletedCount = await db.RefreshTokens
                        .Where(t => t.ExpiresAt < cutoffDate || (t.RevokedAt != null && t.RevokedAt < cutoffDate))
                        .ExecuteDeleteAsync(cancellationToken);

                    _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens.", deletedCount);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "An error occurred while cleaning up expired tokens.");
                }
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }
    }
}
