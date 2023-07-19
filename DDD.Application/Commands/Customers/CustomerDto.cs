﻿using DDD.Application.Mapper.Dtos;
using DDD.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Customers
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public CustomerNameDto Name { get; set; }
        public ContactDto Contact { get; set; }
        public AddressDto Address { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
