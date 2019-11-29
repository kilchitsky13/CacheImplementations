namespace CacheExample.Interfaces
{
    public interface ICacheService<TKey, TValue> where TKey : class where TValue : new()
    {
        ICacheResult<TValue> TryGet(TKey key);
        ICacheResult TryAdd(TKey key, TValue model);
        ICacheResult TryRemove(TKey key);
    }
}