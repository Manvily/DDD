using AutoMapper;
using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Orders;
using DDD.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Mapper
{
    internal class MappingRequests : Profile
    {
        public MappingRequests()
        {
            CreateMap<CustomerCreateCommand, Customer>();
            CreateMap<OrderCreateCommand, Order>();
        }
    }
}
