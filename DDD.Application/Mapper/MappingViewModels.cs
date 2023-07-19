using AutoMapper;
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
            //CreateMap<Customer, CustomerViewModel>()
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact))
            //    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            //    .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate));
        }
    }
}
