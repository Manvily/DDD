using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DDD.Infrastructure.Queries
{
    internal class ProductsQueryRepository : IProductsQueryRepository
    {
        private readonly SqlServerContext _context;

        public ProductsQueryRepository(SqlServerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(x => x.Category).ToListAsync();
        }
    }
}
