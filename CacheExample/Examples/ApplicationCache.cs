using System;
using System.Collections.Generic;

namespace CacheExample.Examples
{
    public class ApplicationCache<TKey, TValue> where TValue : new()
    {
        private class Value<T>
        {
            public T Data { get; private set; }

            public long Time { get; private set; }

            public Value(T data)
            {
                Data = data;
                Time = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
            }

            public bool IsValid(uint cacheTime)
            {
                return (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) - Time < cacheTime;
            }
        }

        private readonly Dictionary<TKey, Value<TValue>> _cache = new Dictionary<TKey, Value<TValue>>();
        private readonly uint _cacheTime;

        public ApplicationCache(uint cacheTime)
        {
            _cacheTime = cacheTime;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    if (item.IsValid(_cacheTime))
                    {
                        value = item.Data;
                        return true;
                    }
                    else
                    {
                        _cache.Remove(key);
                    }
                }
            }

            value = default(TValue);
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_cache)
            {
                _cache.Remove(key);
                _cache.Add(key, new Value<TValue>(value));
            }
        }
    }
}
