using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Customers;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;

namespace DDD.Infrastructure.Commands
{
    internal class CustomersCommandRepository : BaseCommandRepository, ICustomersCommandRepository
    {
        private readonly SqlServerContext _context;

        public CustomersCommandRepository(SqlServerContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> Create(Customer customerEntity)
        {
            var result = await CreateEntity<Guid>(customerEntity);

            return result;
        }

        public async Task<Customer?> FindAsync(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }
    }
}
