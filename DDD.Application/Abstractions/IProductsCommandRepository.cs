using DDD.Domain.Entities;

namespace DDD.Application.Abstractions
{
    public interface IProductsCommandRepository
    {
        Task<IEnumerable<Product>> FindMany(IEnumerable<Guid> ids);
    }
}