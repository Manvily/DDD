using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Categories
{
    public class CategoryDto
    {
        public NameValueDto Name { get; }
    }
}
