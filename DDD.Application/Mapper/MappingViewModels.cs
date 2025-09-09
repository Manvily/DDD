using AutoMapper;
using DDD.Application.Queries.Customers;
using DDD.Application.Queries.Orders;
using DDD.Domain.Entities;
using DDD.Application.Queries.Products;

namespace DDD.Application.Mapper
{
    public class MappingViewModels : Profile
    {
        public MappingViewModels()
        {
            CreateMap<Customer, CustomerViewModel>().ReverseMap();
            CreateMap<Product, ProductViewModel>()
                .ForMember(x => x.Price, opt => opt.MapFrom(src => src.Price.Value))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name.Name))
                .ForMember(x => x.CategoryName, opt => opt.MapFrom(src => src.Category.Name.Name))
                .ForMember(x => x.CategoryId, opt => opt.MapFrom(src => src.Category.Id));

            CreateMap<Order, OrderViewModel>()
                .ForMember(x => x.CustomerId, opt => opt.MapFrom(src => src.Customer.Id))
                .ForMember(x => x.IsPaid, opt => opt.MapFrom(src => src.Payment.IsPaid));
        }
    }
}