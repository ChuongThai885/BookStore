using BookStore.Common.Caching;
using Cart.Application.Interfaces;
using Cart.Application.Services;
using Cart.Domain.Interfaces;
using Cart.Infrastructure.Data;
using Cart.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace Cart.API.Extension
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers infrastructure services: DbContext and repositories.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CartConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'CartConnection' is required.");
            }

            // Configure EF Core DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // register redis cache services
            services.AddSharedRedis(configuration);

            // Repository registrations
            services.AddScoped<ICartRepository, CartRepository>();

            return services;
        }
        /// <summary>
        /// Registers application services: AutoMapper profiles and business services.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Service registrations
            services.AddScoped<ICartService, CartService>();

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

            return services;
        }
        public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            var authAuthority = configuration["Authentication:Authority"];
            var validAudience = configuration["Jwt:Audience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authAuthority;
                    options.RequireHttpsMetadata = true;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authAuthority,
                        ValidateAudience = true,
                        ValidAudience = validAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromSeconds(15)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.TryGetValue("access_token", out var accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };

                });

            services.AddAuthorization();
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure(configuration)
                .AddApplication()
                .AddWebApi()
                .AddAuthenticationService(configuration);

            return services;
        }
    }
}
