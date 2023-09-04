using DDD.Application.Commands.Customers;
using DDD.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Abstractions
{
    public interface ICustomersCommandRepository
    {
        Task<bool> Create(Customer customerEntity);
        Task<Customer?> FindAsync(Guid id);
    }
}
