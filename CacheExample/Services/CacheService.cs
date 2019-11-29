using System;
using System.Threading.Tasks;
using CacheExample.Extensions;
using CacheExample.Interfaces;
using CacheExample.Managers;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CacheExample.Services
{
    public class CacheService<TModel> where TModel : new()
    {
        private readonly RedisCacheManager<TModel> _redisCacheManager;
        private readonly InMemoryCacheManager<TModel> _inMemoryCache;

        public CacheService(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            if (bool.TryParse(configuration["RedisCache:Enabled"], out var redisCacheEnabled) &&
                redisCacheEnabled)
            {
                _redisCacheManager = new RedisCacheManager<TModel>(configuration);
            }

            if (bool.TryParse(configuration["InMemoryCache:Enabled"], out var inMemoryCacheEnabled) &&
                inMemoryCacheEnabled)
            {
                _inMemoryCache = new InMemoryCacheManager<TModel>(memoryCache, configuration);
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
                AddModelToCache(key, resaltSearchInDB);
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
                var resaltSearchInRedisCache =  _redisCacheManager.TryGet(key);
                if (resaltSearchInRedisCache.IsSuccess)
                {
                    _inMemoryCache?.TryAdd(key, resaltSearchInRedisCache.Data);
                    return new CacheResult<TModel>(resaltSearchInRedisCache.Data);
                }
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }

        private async void AddModelToCache(string key, TModel model)
        {
            if (model != null)
            {
                _inMemoryCache?.TryAdd(key, model);

                if (_redisCacheManager != null)
                    _redisCacheManager.TryAdd(key, model);
            }
        }


        
        #endregion
    }
}
