using DDD.Application.Queries.Customers;
using DDD.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Abstractions
{
    public interface ICustomersQueryRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
    }
}
