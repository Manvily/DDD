using MediatR;

namespace DDD.Application.Queries.Orders;

public class CustomerOrdersQuery : IRequest<IEnumerable<OrderViewModel>>
{
    public Guid CustomerId { get; set; }
}