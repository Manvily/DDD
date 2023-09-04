namespace DDD.Application.Cache;

public interface IRedisCache
{
    Task<T> GetAsync<T>(string key, Func<Task<T>> func);
    Task RemoveAsync(string key);
}