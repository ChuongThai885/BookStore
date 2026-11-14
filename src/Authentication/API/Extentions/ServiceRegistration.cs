using Authentication.API.Infrastructure;
using Authentication.Application.Interfaces;
using Authentication.Application.Services;
using Authentication.Domain.Constants;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Options;
using Authentication.Infrastructure.BackgroundJobs;
using Authentication.Infrastructure.Data;
using Authentication.Infrastructure.Repository;
using Authentication.Infrastructure.Security;
using BookStore.Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;

namespace Authentication.API.Extentions
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers infrastructure services: DbContext and repositories.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AuthenticationConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'AuthenticationConnection' is required.");
            }

            // Configure EF Core DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register redis cache service
            services.AddSharedRedis(configuration);

            // Repository registrations
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
        /// <summary>
        /// Registers application services: AutoMapper profiles and business services.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Service registrations
            services.AddScoped<ITokenServices, TokenServices>();
            services.AddScoped<IAuthServices, AuthServices>();

            return services;
        }
        /// <summary>
        /// Registers Web API specific services: controllers, Swagger, API behavior.
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<ApplicationUserEntity>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<JwtKeyManager, IOptions<JwtOptions>>((options, keyManager, jwtOptions) =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Value.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Value.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = keyManager.PublicJwk,
                        ClockSkew = TimeSpan.FromSeconds(15)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token) &&
                            context.Request.Cookies.TryGetValue(AuthConstants.Cookies.AccessToken, out var accessToken))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                            if (!string.IsNullOrEmpty(jti))
                            {
                                var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                                if (await cache.ExistsAsync($"blacklist:jti:{jti}"))
                                {
                                    context.Fail("This access token has been revoked.");
                                }
                            }
                        }
                    };
                });

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ExternalAuthOptions>()
                .BindConfiguration(ExternalAuthOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<JwtKeyManager>();
            services.AddSingleton<IExternalAuthValidator, GoogleAuthValidator>();
            services.AddSingleton<IExternalAuthValidator, MicrosoftAuthValidator>();

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("AuthPolicy", httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetSlidingWindowLimiter(ipAddress, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(60),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                    });
                });
            });

            return services;
        }
        /// <summary>
        /// Registers Web API specific services: controllers, Swagger, API behavior.
        /// </summary>
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddRouting(options => options.LowercaseUrls = true);

            return services;
        }
        /// <summary>
        /// Registers Global exception handler.
        /// </summary>
        public static IServiceCollection AddGlobalExeptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
        /// <summary>
        /// Registers background services: hosted services for periodic tasks.
        /// </summary>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<TokenCleanupBackgroundService>();

            return services;
        }
        /// <summary>
        /// Centralized method to register all layers in correct order.
        /// </summary>
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure(configuration)
                    .AddApplication()
                    .AddAuthenticationServices(configuration)
                    .AddWebApi()
                    .AddGlobalExeptionHandler()
                    .AddBackgroundServices();

            return services;
        }
    }
}
