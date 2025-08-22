using MediatR;
using Shared.Domain.Events;

namespace DDD.Application.Cache.Orders;

public class RemoveCustomerOrdersQueryCacheHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IRedisCache _redisCache;
    
    public RemoveCustomerOrdersQueryCacheHandler(IRedisCache redisCache)
    {
        _redisCache = redisCache;
    }
    
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _redisCache.RemoveAsync(CacheKeys.CustomerOrders(notification.CustomerId));
    }
}