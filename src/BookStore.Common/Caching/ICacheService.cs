using System;
using System.Collections.Generic;
using System.Text;

namespace BookStore.Common.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string cacheKey);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<long> HashIncrementAsync(string key, string hashField, long value = 1);
        Task<long> HashDecrementAsync(string key, string hashField, long value = 1);
        Task<Dictionary<string, long>> HashGetAllAsync(string key);
        Task HashDeleteAsync(string key, string hashField);
        Task KeyExpireAsync(string key, TimeSpan expiration);
    }
    public interface IDistributedLockService
    {
        Task<IAsyncDisposable?> AcquireLockAsync(string resourceKey, TimeSpan expiration);
    }
}
