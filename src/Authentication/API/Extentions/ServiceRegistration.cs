using Authentication.Application.Interfaces;
using Authentication.Application.Services;
using Authentication.Domain.Entity;
using Authentication.Domain.Interfaces;
using Authentication.Infrastructure.Data;
using Authentication.Infrastructure.Repository;
using Authentication.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            
            // Configure User Identity
            services.AddIdentity<ApplicationUserEntity, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

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
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddSingleton<JwtKeyManager>();
            services.AddScoped<IExternalAuthValidator, GoogleAuthValidator>();
            services.AddSingleton<IExternalAuthValidator, MicrosoftAuthValidator>();

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
                    .AddWebApi();

            return services;
        }
    }
}
