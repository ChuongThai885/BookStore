using Authentication.Application.Mapping;
using Authentication.Domain.Entity;
using Authentication.Infrastructure.Data;
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
            // Configure EF Core DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            
            // Configure User Identity
            services.AddIdentity<ApplicationUserEntity, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Repository registrations
            //services.AddScoped<IAuthorRepository, AuthorRepository>();

            return services;
        }

        /// <summary>
        /// Registers application services: AutoMapper profiles and business services.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper configuration
            services.AddAutoMapper(typeof(MappingProfile));

            // Service registrations
            //services.AddScoped<IAuthorService, AuthorService>();

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
                    .AddWebApi();

            return services;
        }
    }
}
