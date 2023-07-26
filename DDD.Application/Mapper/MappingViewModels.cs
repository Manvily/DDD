using AutoMapper;
using DDD.Application.Commands.Customers;
using DDD.Application.Mapper.Dtos;
using DDD.Application.Queries.Customers;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Mapper
{
    internal class MappingViewModels : Profile
    {
        public MappingViewModels()
        {
            CreateMap<Customer, CustomerViewModel>().ReverseMap();
        }
    }
}
