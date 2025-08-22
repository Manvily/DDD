using MediatR;
using Shared.Domain.Events;

namespace DDD.Application.Cache.Customers;

public class RemoveCustomersListQueryCacheHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly IRedisCache _redisCache;
    
    public RemoveCustomersListQueryCacheHandler(IRedisCache redisCache)
    {
        _redisCache = redisCache;
    }
    
    public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _redisCache.RemoveAsync(CacheKeys.CustomersList);
    }
}