using System;
using System.Threading.Tasks;
using CacheExample.Configuration;
using CacheExample.Enums;
using CacheExample.Helpers;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;

namespace CacheExample.Examples
{
    public class DistributedCache<TModel> where TModel : class
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly string _keyPrefix;
        private static long _lastUnhealthTime;

        public DistributedCache(IConfigurationRoot configuration)
        {
            var conf = ConnectionMultiplexerConfigs.CreateConfigs(configuration);

            if (ConnectionMultiplexerConfigs.Health != RedisCacheHealthStatus.Unhealth)
            {
                try
                {
                    _connectionMultiplexer = ConnectionMultiplexer.Connect(conf.Value);
                    _keyPrefix = $"{_connectionMultiplexer.ClientName}:{typeof(TModel).Name}";

                    ConnectionMultiplexerConfigs.SetHealthy();
                }
                catch (RedisConnectionException rcex)
                {
                    ConnectionMultiplexerConfigs.SetUnHealthy();
                    Log.Error($"Could not connect to redis:{rcex.Message}");
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                }
            }
        }

        public async Task<ICacheResult<TModel>> TryGet(string key)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();
                    var getResult = await db.StringGetAsync($"{_keyPrefix}:{key}");

                    if (getResult.HasValue)
                    {
                        var found = JsonHelper.Deserialize<TModel>(getResult);
                        return new CacheResult<TModel>(found);
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult<TModel>(e);
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }

        public async Task TryAdd(string key, TModel model)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();

                    var json = JsonHelper.Serialize(model);

                    var setResult = await db.StringSetAsync($"{_keyPrefix}:{key}", json, TimeSpan.FromSeconds(ConnectionMultiplexerConfigs.AbsoluteExpirationRelativeToNow));

                    if (!setResult)
                    {
                        var isExist =  await db.KeyExistsAsync($"{_keyPrefix}:{key}");
                        if (!isExist)
                        {
                            var info = $"Object with key:{_keyPrefix}:{key} was not recorded";
                            Log.Information(info);
                        }
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }


        public async Task RemoveByKey(string key)
        {
            var keyForDelete = string.IsNullOrEmpty(key)
                ? _keyPrefix
                : $"{_keyPrefix}:{key}";

            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();
                    var deleteRes = await db.KeyDeleteAsync(keyForDelete);

                    if (!deleteRes)
                    {
                        var isExist = await db.KeyExistsAsync(keyForDelete);
                        if (isExist)
                            Log.Information($"Key:{keyForDelete} was not deleted!");
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

    }
}
