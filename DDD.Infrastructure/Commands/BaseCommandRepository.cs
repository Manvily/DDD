using DDD.Infrastructure.EntityFramework;
using Shared.Domain.Core;

namespace DDD.Infrastructure.Commands
{
    internal abstract class BaseCommandRepository
    {
        private readonly SqlServerContext _context;

        public BaseCommandRepository(SqlServerContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateEntity<T>(Entity<T> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Add(entity);
            var created = await _context.SaveChangesAsync();

            return created > 0;
        }
    }
}
