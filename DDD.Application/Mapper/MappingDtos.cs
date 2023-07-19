using AutoMapper;
using DDD.Application.Commands.Categories;
using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Orders;
using DDD.Application.Commands.Products;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Mapper
{
    internal class MappingDtos : Profile
    {
        public MappingDtos()
        {
            // Entities
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Order, OrderDto>().ReverseMap();

            // Value Objects
            CreateMap<PaymentStatus, PaymentStatusDto>().ReverseMap();
            CreateMap<NameValue, NameValueDto>().ReverseMap();
            CreateMap<CustomerName, CustomerNameDto>().ReverseMap();
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Contact, ContactDto>().ReverseMap();
            CreateMap<Price, PriceDto>().ReverseMap();
        }
    }
}
