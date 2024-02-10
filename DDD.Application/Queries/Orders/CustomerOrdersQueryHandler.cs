using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Cache;
using DDD.Application.Exceptions;
using MediatR;

namespace DDD.Application.Queries.Orders;

public class CustomerOrdersQueryHandler : IRequestHandler<CustomerOrdersQuery, IEnumerable<OrderViewModel>>
{
    private readonly IOrdersQueryRepository _ordersQueryRepository;
    private readonly IMapper _mapper;
    private readonly IRedisCache _redisCache;

    public CustomerOrdersQueryHandler(IOrdersQueryRepository ordersQueryRepository, IMapper mapper,
        IRedisCache redisCache)
    {
        _ordersQueryRepository = ordersQueryRepository;
        _mapper = mapper;
        _redisCache = redisCache;
    }

    public async Task<IEnumerable<OrderViewModel>> Handle(CustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var customerList = await _redisCache.GetAsync(CacheKeys.CustomerOrders(request.CustomerId),
            async () => await _ordersQueryRepository.GetCustomerOrdersAsync(request.CustomerId));

        if (!customerList.Any())
        {
            throw new NotExistsException("The resource was not found")
            {
                Ids = new[] { request.CustomerId }
            };
        }
        
        return _mapper.Map<IEnumerable<OrderViewModel>>(customerList);
    }
}