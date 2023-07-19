using DDD.Application.Commands.Categories;
using DDD.Application.Commands.Orders;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Products
{
    public class ProductDto
    {
        public NameValueDto Name { get; set; }
        public PriceDto Price { get; set; }
        public CategoryDto Category { get; set; }
    }
}
