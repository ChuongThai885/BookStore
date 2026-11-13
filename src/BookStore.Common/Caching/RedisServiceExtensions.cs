using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Caching
{
    public static class RedisServiceExtensions
    {
        public static IServiceCollection AddSharedRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RedisConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'RedisConnection' is required.");
            }

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RedisCacheService>());

            return services;
        }
    }
}
