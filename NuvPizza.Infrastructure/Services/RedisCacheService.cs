using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using NuvPizza.Domain.Interfaces;
using StackExchange.Redis;

namespace NuvPizza.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedResponse = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedResponse))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedResponse);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromHours(1),
                SlidingExpiration = unusedExpireTime
            };

            var serializedResponse = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedResponse, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefixKey)
        {
            foreach (var endPoint in _connectionMultiplexer.GetEndPoints())
            {
                var server = _connectionMultiplexer.GetServer(endPoint);
                var keys = server.Keys(pattern: prefixKey + "*");
                
                var db = _connectionMultiplexer.GetDatabase();

                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
            }
        }
    }
}
