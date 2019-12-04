using System;
using CacheExample.Configuration;
using CacheExample.Helpers;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;

namespace CacheExample.Managers
{
    public class RedisCacheManager<TValue> : ICacheManager<string, TValue>, IDisposable where TValue : new()
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheManager(IConfigurationRoot config)
        {
            var conOptions = ConnectionMultiplexerConfigs.CreateConfigs(config);
            _connectionMultiplexer = ConnectionMultiplexer.Connect(conOptions.Value);
        }

        public ICacheResult<TValue> TryGet(string key)
        {
            try
            {
                var getResult = _connectionMultiplexer.GetDatabase().StringGet(key);

                if (getResult.HasValue)
                {
                    var found = JsonHelper.Deserialize<TValue>(getResult);
                    return new CacheResult<TValue>(found);
                }
            }
            catch (RedisConnectionException rcex)
            {
                Log.Error($"Could not connect to redis:{rcex.Message}");
                return new CacheResult<TValue>(rcex);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult<TValue>(e);
            }
            return new CacheResult<TValue>("ItemDoesNotExist");
        }

        public ICacheResult TryAdd(string key, TValue model)
        {
            try
            {
                var db = _connectionMultiplexer.GetDatabase();
                var json = JsonHelper.Serialize(model);
                db.StringSet(key, json, TimeSpan.FromSeconds(ConnectionMultiplexerConfigs.AbsoluteExpirationRelativeToNow));
                return new CacheResult();

            }
            catch (RedisConnectionException rcex)
            {
                Log.Error($"Could not connect to redis:{rcex.Message}");
                return new CacheResult(rcex);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult(e);
            }
        }

        public ICacheResult TryRemove(string key)
        {
            try
            {
                var db = _connectionMultiplexer.GetDatabase();
                db.KeyDelete(key);
                return new CacheResult();
            }
            catch (RedisConnectionException rcex)
            {
                Log.Error($"Could not connect to redis:{rcex.Message}");
                return new CacheResult(rcex);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult(e);
            }
        }
        
        public void Dispose()
        {
            _connectionMultiplexer?.Dispose();
        }
    }
}
