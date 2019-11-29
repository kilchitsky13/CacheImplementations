namespace CacheExample.Interfaces
{
    public interface ICacheResult
    {
        bool IsSuccess { get; set; }
        string ErrorMessage { get; set; }
    }


    public interface ICacheResult<T> : ICacheResult
    {
        T Data { get; set; }
    }
}
