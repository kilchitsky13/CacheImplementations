using System;
using System.Threading.Tasks;
using CacheExample.Interfaces;
using CacheExample.Managers;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CacheExample.Services
{
    public abstract class MultiLayerCacheService<TModel> where TModel : ModelWithId<string>, new()
    {
        private readonly RedisCacheManager<TModel> _redisCacheManager;
        private readonly InMemoryCacheManager<TModel> _inMemoryCache;

        public MultiLayerCacheService(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            _redisCacheManager = new RedisCacheManager<TModel>(configuration);
            _inMemoryCache = new InMemoryCacheManager<TModel>(memoryCache, configuration);
        }

        public abstract Task<CacheResult<TModel>> GetData(Func<string, Task<TModel>> func, string parameter);
        public abstract Task<CacheResult<TModel>> AddData(string key, TModel model);

        #region Protected

        protected async Task<ICacheResult<TModel>> GetObjectFromCache(string key)
        {
            var resultSearchInMemoryCache = _inMemoryCache.TryGet(key);
            if (resultSearchInMemoryCache.IsSuccess)
            {
                return new CacheResult<TModel>(resultSearchInMemoryCache.Data);
            }

            if (_redisCacheManager != null)
            {
                var resultSearchInRedisCache = _redisCacheManager.TryGet(key);
                if (resultSearchInRedisCache.IsSuccess)
                {
                    _inMemoryCache?.TryAdd(key, resultSearchInRedisCache.Data);
                    return new CacheResult<TModel>(resultSearchInRedisCache.Data);
                }
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }

        protected async void AddModelToCache(string key, TModel model)
        {
            if (model == null)
                return;

            var addResult = _inMemoryCache?.TryAdd(key, model);


            _redisCacheManager?.TryAdd(key, model);
        }


        protected string GetKey(string id)
        {
            return $"{typeof(TModel).Name}_{id}";
        }

        protected string GetKey(TModel model)
        {
            return GetKey(model.Id);
        }

        #endregion

    }
}
