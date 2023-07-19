using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Commands
{
    internal class ProductsCommandRepository : BaseCommandRepository, IProductsCommandRepository
    {
        private readonly SqlServerContext _context;

        public ProductsCommandRepository(SqlServerContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> FindMany(IEnumerable<Guid> ids)
        {
            return await _context.Products.Where(x => ids.Contains(x.Id)).ToListAsync();
        }
    }
}
