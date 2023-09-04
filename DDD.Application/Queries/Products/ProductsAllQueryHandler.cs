using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Cache;
using MediatR;

namespace DDD.Application.Queries.Products;

public class ProductsAllQueryHandler : IRequestHandler<ProductsAllQuery, IEnumerable<ProductViewModel>>
{
    private readonly IProductsQueryRepository _productsQueryRepository;
    private readonly IMapper _mapper;
    private readonly IRedisCache _redisCache;
    
    public ProductsAllQueryHandler(IProductsQueryRepository productsQueryRepository, IMapper mapper,
        IRedisCache redisCache)
    {
        _productsQueryRepository = productsQueryRepository;
        _mapper = mapper;
        _redisCache = redisCache;
    }
    
    public async Task<IEnumerable<ProductViewModel>> Handle(ProductsAllQuery request, CancellationToken cancellationToken)
    {
        var results = await _redisCache.GetAsync(CacheKeys.ProductsList,
            async () => await _productsQueryRepository.GetAllAsync());
        return _mapper.Map<IEnumerable<ProductViewModel>>(results);
    }
}
