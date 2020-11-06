using System;
using Newtonsoft.Json;
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

    public class CacheClient : ICacheClient
    {
        private Lazy<IConnectionMultiplexer> _connection = null;

        public CacheClient(string connectionString, bool allowAdmin = false)
        {
            var configuration = ConfigurationOptions.Parse(connectionString);
            configuration.AllowAdmin = allowAdmin;
            configuration.ReconnectRetryPolicy = new LinearRetry(5000);
            configuration.AbortOnConnectFail = false;

            Construct(configuration);
        }

        public CacheClient(ConfigurationOptions options)
        {
            Construct(options);
        }

        public CacheClient(string host = "localhost", int port = 6379, bool allowAdmin = false)
        {
            var configuration = new ConfigurationOptions
            {
                //for the redis pool so you can extent later if needed
                EndPoints = { { host, port }, },
                AllowAdmin = allowAdmin,
                //Password = "", //to the security for the production
                ClientName = "My Redis Client",
                ReconnectRetryPolicy = new LinearRetry(5000),
                AbortOnConnectFail = false,
            };
            
            Construct(configuration);
        }

        private void Construct(ConfigurationOptions options)
        {
            _connection = new Lazy<IConnectionMultiplexer>(() =>
            {
                var redis = ConnectionMultiplexer.Connect(options);
                //redis.ErrorMessage += _Connection_ErrorMessage;
                //redis.InternalError += _Connection_InternalError;
                //redis.ConnectionFailed += _Connection_ConnectionFailed;
                //redis.ConnectionRestored += _Connection_ConnectionRestored;
                return redis;
            });
        }
        
        //for the 'GetSubscriber()' and another Databases
        public IConnectionMultiplexer Connection => _connection.Value;

        //for the default database
        public IDatabase Database => Connection.GetDatabase();

        public T JsonGet<T>(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var rv = Database.StringGet(key, flags);
            if (!rv.HasValue)
                return default;
            var rgv = JsonConvert.DeserializeObject<T>(rv);
            return rgv;
        }

        public bool JsonSet(RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            return value != null 
                   && 
                   Database.StringSet(key, JsonConvert.SerializeObject(value), expiry, when, flags);
        }

        private void _Connection_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
