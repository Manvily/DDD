namespace DDD.Application.Cache;

public interface IRedisCache
{
    Task<T> GetAsync<T>(CacheKeys key, Func<Task<T>> func);
    Task RemoveAsync(CacheKeys key);
}