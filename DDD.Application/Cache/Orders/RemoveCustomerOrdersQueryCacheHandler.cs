using DDD.Domain.Events;
using MediatR;

namespace DDD.Application.Cache.Orders;

public class RemoveCustomerOrdersQueryCacheHandler : INotificationHandler<OrderAdded>
{
    private readonly IRedisCache _redisCache;
    
    public RemoveCustomerOrdersQueryCacheHandler(IRedisCache redisCache)
    {
        _redisCache = redisCache;
    }
    
    public async Task Handle(OrderAdded notification, CancellationToken cancellationToken)
    {
        await _redisCache.RemoveAsync(CacheKeys.CustomerOrders(notification.CustomerId));
    }
}