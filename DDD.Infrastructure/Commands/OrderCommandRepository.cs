using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;

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
