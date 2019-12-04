using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CacheExample.Services
{
    public class WriteBehindCacheService<TModel> : MultiLayerCacheService<TModel> where TModel : ModelWithId<string>, new()
    {
        private readonly List<string> PendingToSaveItemKeys = new List<string>();
        
        public WriteBehindCacheService(IMemoryCache memoryCache, IConfigurationRoot configuration) : base(memoryCache, configuration)
        {
        }

        public override async Task<CacheResult<TModel>> GetData(Func<string, Task<TModel>> func, string parameter)
        {
            var key = GetKey(parameter);

            var getFromCachResult = await GetObjectFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new CacheResult<TModel>(getFromCachResult.Data);
            }

            var funcGetResult = await func(parameter);
            if (funcGetResult != null)
            {
                AddModelToCache(key, funcGetResult);
                return new CacheResult<TModel>(funcGetResult);
            }
            return new CacheResult<TModel>("ItemDoesNotExist");
        }

        public override async Task<CacheResult<TModel>> AddData(string key, TModel model)
        {
            await Task.Run(() => AddModelToCache(key, model));
            return new CacheResult<TModel>(model);
        }

        public async Task<CacheResult<TModel>> AddData(Func<TModel, Task<TModel>> func, TModel model)
        {
            var funcAddResult = await func(model);
            if (funcAddResult != null)
            {
                var key = GetKey(model);
                AddModelToCache(key, model);

                return new CacheResult<TModel>(funcAddResult);
            }

            return new CacheResult<TModel>();
        }


    }
}