using MediatR;

namespace DDD.Application.Queries.Products;

public class ProductsAllQuery : IRequest<IEnumerable<ProductViewModel>>
{
    
}