using DDD.Domain.Entities;

namespace DDD.Application.Abstractions
{
    public interface IProductsQueryRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
    }
}
