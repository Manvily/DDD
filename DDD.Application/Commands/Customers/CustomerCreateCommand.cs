using DDD.Application.Mapper.Dtos;
using DDD.Application.Queries.Customers;
using DDD.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Customers
{
    public class CustomerCreateCommand : IRequest<CustomerDto>
    {
        public CustomerNameDto Name { get; set; }
        public ContactDto Contact { get; set; }
        public AddressDto Address { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
