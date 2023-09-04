using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Cache;
using MediatR;

namespace DDD.Application.Queries.Customers
{
    internal class CustomersAllQueryHandler : IRequestHandler<CustomersAllQuery, IEnumerable<CustomerViewModel>>
    {
        private readonly ICustomersQueryRepository _customersQueryRepository;
        private readonly IMapper _mapper;
        private readonly IRedisCache _redisCache;

        public CustomersAllQueryHandler(ICustomersQueryRepository customersQueryRepository, IMapper mapper,
            IRedisCache redisCache)
        {
            _customersQueryRepository = customersQueryRepository;
            _mapper = mapper;
            _redisCache = redisCache;
        }

        public async Task<IEnumerable<CustomerViewModel>> Handle(CustomersAllQuery request,
            CancellationToken cancellationToken)
        {
            var customerList = await _redisCache.GetAsync(CacheKeys.CustomersList,
                async () => await _customersQueryRepository.GetAllAsync());

            return _mapper.Map<IEnumerable<CustomerViewModel>>(customerList);
        }
    }
}