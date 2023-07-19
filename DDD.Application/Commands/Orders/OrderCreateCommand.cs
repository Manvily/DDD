using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Products;
using DDD.Application.Mapper.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Orders
{
    public class OrderCreateCommand : IRequest<OrderDto>
    {
        public Guid CustomerId { get; set; }
        public IEnumerable<Guid> ProductsIds { get; set; }
    }
}
