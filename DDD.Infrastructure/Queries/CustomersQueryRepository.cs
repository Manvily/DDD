using DDD.Application.Abstractions;
using DDD.Application.Queries.Customers;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Queries
{
    internal class CustomersQueryRepository : ICustomersQueryRepository
    {
        private readonly SqlServerContext _context;

        public CustomersQueryRepository(SqlServerContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }
    }
}
