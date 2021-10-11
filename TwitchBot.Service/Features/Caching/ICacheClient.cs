using System;
using StackExchange.Redis;

namespace TwitchBot.Service.Features.Caching
{
    public interface ICacheClient
    {
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
        T JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None);
        bool JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None);
    }
}