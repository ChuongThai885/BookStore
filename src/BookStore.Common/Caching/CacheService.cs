using Microsoft.Extensions.Configuration;
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
        public async Task<long> HashIncrementAsync(string key, string hashField, long value = 1)
        {
            return await _database.HashIncrementAsync(key, hashField, value);
        }
        public async Task<long> HashDecrementAsync(string key, string hashField, long value = 1)
        {
            return await _database.HashDecrementAsync(key, hashField, value);
        }
        public async Task<Dictionary<string, long>> HashGetAllAsync(string key)
        {
            var entries = await _database.HashGetAllAsync(key);

            var result = new Dictionary<string, long>();
            foreach(var entry in entries)
            {
                if(long.TryParse(entry.Value.ToString(), out var qty))
                {
                    result[entry.Name.ToString()] = qty;
                }
            }
            return result;
        }
        public async Task HashDeleteAsync(string key, string hashField)
        {
            await _database.HashDeleteAsync(key, hashField);
        }
        public async Task KeyExpireAsync(string key, TimeSpan expiration)
        {
            await _database.KeyExpireAsync(key, expiration);
        }
    }
}
