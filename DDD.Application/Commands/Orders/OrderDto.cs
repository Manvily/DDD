using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Products;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Orders
{
    public class OrderDto
    {
        public CustomerDto Customer { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
        public PaymentStatusDto Payment { get; set; }
    }
}
