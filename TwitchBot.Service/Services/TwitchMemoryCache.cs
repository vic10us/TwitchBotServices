using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace TwitchBot.Service.Services
{
    public class TwitchCacheOptions
    {
        public TwitchCacheOptions()
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
        }

        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);
        public DateTimeOffset AbsoluteExpiration { get; set; }
    }

    public interface ICacheService
    {
        Task<bool> ValueExists(string key);
        Task<T> GetValue<T>(string key);
        Task<T> GetOrSetValue<T>(string key, Func<Task<T>> setter, TwitchCacheOptions cacheOptions = null);
    }

    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> ValueExists(string key)
        {
            var cacheKey = key.ToLower();
            var encodedValue = await _cache.GetAsync(cacheKey);
            return encodedValue != null;
        }

        public async Task<T> GetValue<T>(string key)
        {
            var cacheKey = key.ToLower();
            var encodedValue = await _cache.GetAsync(cacheKey);
            if (encodedValue == null) return default;

            var serializedResult = Encoding.UTF8.GetString(encodedValue);
            var result = JsonConvert.DeserializeObject<T>(serializedResult);

            return result;
        }

        public async Task<T> GetOrSetValue<T>(string key, Func<Task<T>> setter, TwitchCacheOptions cacheOptions = null)
        {
            var cacheKey = key.ToLower();
            string serializedResult;
            T result;
            var encodedValue = await _cache.GetAsync(cacheKey);

            if (encodedValue != null)
            {
                serializedResult = Encoding.UTF8.GetString(encodedValue);
                result = JsonConvert.DeserializeObject<T>(serializedResult);
            }
            else
            {
                result = await setter();
                serializedResult = JsonConvert.SerializeObject(result);
                encodedValue = Encoding.UTF8.GetBytes(serializedResult);
                cacheOptions ??= new TwitchCacheOptions();

                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(cacheOptions.SlidingExpiration);
                    // TODO: Figure out why sliding and absolute can't coexist... :(
                    //.SetAbsoluteExpiration(cacheOptions.AbsoluteExpiration);

                await _cache.SetAsync(cacheKey, encodedValue, options);
            }

            return result;
        }
    }

    public class TwitchMemoryCache
    {
        public MemoryCache Cache { get; set; }
        public TwitchMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
        }
    }
}
