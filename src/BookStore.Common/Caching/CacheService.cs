∩╗┐using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookStore.Common.Caching
{
    public class RedisCacheService: ICacheService
    {
        private readonly IDatabase _database;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        public RedisCacheService(IConnectionMultiplexer muxer, IConfiguration config)
        {
            _database = muxer.GetDatabase();
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan epiration)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, json, epiration);
        }
        public async Task RemoveAsync(string key) => await _database.KeyDeleteAsync(key);
        public async Task<bool> ExistsAsync(string key) => await _database.KeyExistsAsync(key);
    }
}
