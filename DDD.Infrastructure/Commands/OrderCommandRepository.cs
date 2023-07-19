using DDD.Application.Abstractions;
using DDD.Domain.Core;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Commands
{
    internal class OrderCommandRepository : BaseCommandRepository, IOrderCommandRepository
    {
        private readonly SqlServerContext _context;

        public OrderCommandRepository(SqlServerContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CreateOrder(Order orderEntity)
        {
            var result = await CreateEntity<Guid>(orderEntity);
            return result;
        }
    }
}
