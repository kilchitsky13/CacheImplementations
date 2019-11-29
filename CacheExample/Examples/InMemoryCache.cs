using System;
using System.Threading;
using System.Threading.Tasks;
using CacheExample.Interfaces;
using CacheExample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace CacheExample.Examples
{
    public class InMemoryCache<TModel> where TModel : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public InMemoryCache(IMemoryCache memoryCache, IConfigurationRoot configuration)
        { 
            if (bool.TryParse(configuration["InMemoryCache:Enabled"], out var enabled) && enabled)
            {
                _memoryCache = memoryCache;

                if (int.TryParse(configuration["InMemoryCache:Options:AbsoluteExpirationRelativeToNow"],
                    out var absoluteExpirationRelativeToNow))
                {
                    _memoryCacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(absoluteExpirationRelativeToNow != 0 ? absoluteExpirationRelativeToNow : 20),
                        ExpirationTokens = { new CancellationChangeToken(_cts.Token) }
                    };
                }
            }
        }

        public ICacheResult<TModel> TryGet(string key)
        {
            try
            {
                if (_memoryCache != null && _memoryCache.TryGetValue(key, out TModel found))
                {
                    return new CacheResult<TModel>(found);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult<TModel>(e);
            }
            return new CacheResult<TModel>("Item does not exist");
        }

        public void TryAdd(string key, TModel model)
        {
            try
            {
                if (_memoryCache != null && model != null)
                {
                    _memoryCache.Set(key, model, _memoryCacheEntryOptions);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

       
        public async Task TryRemove(string key)
        {
            try
            {
                await Task.Run(() => _memoryCache.Remove(key));
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public async Task RemoveAllAsync()
        {
            try
            {
                await Task.Run(() => _cts.Cancel());
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }
    }
}
