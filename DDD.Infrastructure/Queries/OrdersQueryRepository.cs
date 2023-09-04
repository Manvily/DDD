using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DDD.Infrastructure.Queries;

internal class OrdersQueryRepository : IOrdersQueryRepository
{
    private readonly SqlServerContext _context;

    public OrdersQueryRepository(SqlServerContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId)
    {
        return await _context.Orders.Where(x => x.Customer.Id == customerId).ToListAsync();
    }
}