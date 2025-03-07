﻿using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

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
