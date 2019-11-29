using System;
using System.Threading.Tasks;
using CacheExample.Examples;
using CacheExample.Extensions;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CacheExample.Managers
{
    public class CacheManager<TModel> where TModel : class
    {
        private readonly DistributedCache<TModel> _redisCacheManager;
        private readonly InMemoryCache<TModel> _inMemoryCache;

        public CacheManager(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            if (bool.TryParse(configuration["RedisCache:Enabled"], out var redisCacheEnabled) &&
                redisCacheEnabled)
            {
                _redisCacheManager = new DistributedCache<TModel>(configuration);
            }

            if (bool.TryParse(configuration["InMemoryCache:Enabled"], out var inMemoryCacheEnabled) &&
                inMemoryCacheEnabled)
            {
                _inMemoryCache = new InMemoryCache<TModel>(memoryCache, configuration);
            }
        }

        public async Task<ICacheResult<TModel>> Get(Func<string, Task<TModel>> func, string parameter)
        {
            var key = func.GetBodyHashCode(parameter);

            var getFromCachResult = await GetObjectFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new CacheResult<TModel>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(parameter);
            if (resaltSearchInDB != null)
            {
                await AddModelToCache(key, resaltSearchInDB);
                return new CacheResult<TModel>(resaltSearchInDB);
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }


        #region Private


        private async Task<ICacheResult<TModel>> GetObjectFromCache(string key)
        {
            if (_inMemoryCache != null)
            {
                var resaltSearchInMemoryCache = _inMemoryCache.TryGet(key);
                if (resaltSearchInMemoryCache.IsSuccess)
                {
                    return new CacheResult<TModel>(resaltSearchInMemoryCache.Data);
                }
            }

            if (_redisCacheManager != null)
            {
                var resaltSearchInRedisCache = await _redisCacheManager.TryGet(key);
                if (resaltSearchInRedisCache.IsSuccess)
                {
                    _inMemoryCache?.TryAdd(key, resaltSearchInRedisCache.Data);
                    return new CacheResult<TModel>(resaltSearchInRedisCache.Data);
                }
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }

        private async Task AddModelToCache(string key, TModel model)
        {
            if (model != null)
            {
                _inMemoryCache?.TryAdd(key, model);

                if (_redisCacheManager != null)
                    await _redisCacheManager.TryAdd(key, model);
            }
        }


        
        #endregion
    }
}
