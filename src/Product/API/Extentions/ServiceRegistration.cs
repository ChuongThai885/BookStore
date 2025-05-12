using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.Services;
using Product.Domain.Interfaces;
using Product.Infrastructure.Data;
using Product.Infrastructure.Repository;

namespace Product.API.Extentions
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

            // Repository registrations
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();

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
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookService, BookService>();

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
