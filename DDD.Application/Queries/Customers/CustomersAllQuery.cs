﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Queries.Customers
{
    public class CustomersAllQuery : IRequest<IEnumerable<CustomerViewModel>>
    {

    }
}
