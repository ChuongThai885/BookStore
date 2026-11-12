∩╗┐using System;
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
    }
    public interface IDistributedLockService
    {
        Task<IAsyncDisposable?> AcquireLockAsync(string resourceKey, TimeSpan expiration);
    }
}
