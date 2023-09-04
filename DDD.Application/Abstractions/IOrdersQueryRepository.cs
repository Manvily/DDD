using DDD.Application.Queries.Orders;
using DDD.Domain.Entities;

namespace DDD.Application.Abstractions;

public interface IOrdersQueryRepository
{
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId);
}