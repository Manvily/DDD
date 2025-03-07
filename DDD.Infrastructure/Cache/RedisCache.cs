using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Application.Cache;
using Microsoft.Extensions.Caching.Distributed;

namespace DDD.Infrastructure.Cache;

public class RedisCache : IRedisCache
{
    private readonly IDistributedCache _distributedCache;

    public RedisCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }
    
    public async Task<T> GetAsync<T>(string key, Func<Task<T>> func)
    {
        var redisResult = await _distributedCache.GetAsync(key);
        
        if (redisResult == null)
        {
            var results = await func.Invoke();
            SetAsync(key, results);
            return results;
        }
        
        var serializedResults = Encoding.UTF8.GetString(redisResult);
        return JsonSerializer.Deserialize<T>(serializedResults) ?? throw new InvalidOperationException();
    }
    
    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }

    private async void SetAsync<T>(string key, T value)
    {
        var options = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
        };
        var serializedValue = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, options));
        await _distributedCache.SetAsync(key, serializedValue);
    }
}