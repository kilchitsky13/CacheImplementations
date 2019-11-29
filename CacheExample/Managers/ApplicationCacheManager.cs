using System;
using System.Collections.Generic;
using CacheExample.Interfaces;
using CacheExample.Models;
using Serilog;

namespace CacheExample.Managers
{
    public class ApplicationCacheManager<TValue> : ICacheManager<string, TValue> where TValue : new()
    {
        private readonly Dictionary<string, TValue> _cache = new Dictionary<string, TValue>();
        public ApplicationCacheManager()
        {
        }

        public ICacheResult<TValue> TryGet(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    return new CacheResult<TValue>(item);
                }
                return new CacheResult<TValue>("Not found");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult<TValue>(e);
            }
        }

        public ICacheResult TryAdd(string key, TValue model)
        {
            try
            {
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                }
                _cache.Add(key, model);
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
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                    return new CacheResult();
                }
                return new CacheResult("Not found");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new CacheResult(e);
            }
        }
    }
}
