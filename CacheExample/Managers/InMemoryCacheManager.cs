using System;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CacheExample.Managers
{
    public class InMemoryCacheManager<TValue> : ICacheManager<string, TValue> where TValue : new()
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;

        public InMemoryCacheManager(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            _memoryCache = memoryCache;

            if (int.TryParse(configuration["InMemoryCache:Options:AbsoluteExpirationRelativeToNow"],
                out var absoluteExpirationRelativeToNow))
            {
                _memoryCacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(absoluteExpirationRelativeToNow != 0 ? absoluteExpirationRelativeToNow : 10)
                };
            }
        }


        public ICacheResult<TValue> TryGet(string key)
        {
            try
            {
                if (_memoryCache != null && _memoryCache.TryGetValue(key, out TValue found))
                {
                    return new CacheResult<TValue>(found);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult<TValue>(e);
            }
            return new CacheResult<TValue>("Item does not exist");
        }

        public ICacheResult TryAdd(string key, TValue model)
        {
            try
            {
                if (_memoryCache != null && model != null)
                {
                    _memoryCache.Set(key, model, _memoryCacheEntryOptions);
                }
                return new CacheResult();
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
                _memoryCache.Remove(key);
                return new CacheResult();
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult(e);
            }
        }
    }
}
