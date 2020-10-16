using Microsoft.Extensions.Caching.Memory;

namespace TwitchBot.Service.Services
{
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
