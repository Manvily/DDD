using DDD.Domain.Events;
using MediatR;

namespace DDD.Application.Cache.Customers;

public class RemoveCustomersListQueryCacheHandler : INotificationHandler<CustomerAdded>
{
    private readonly IRedisCache _redisCache;
    
    public RemoveCustomersListQueryCacheHandler(IRedisCache redisCache)
    {
        _redisCache = redisCache;
    }
    
    public async Task Handle(CustomerAdded notification, CancellationToken cancellationToken)
    {
        await _redisCache.RemoveAsync(CacheKeys.CustomersList);
    }
}