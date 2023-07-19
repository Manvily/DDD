using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Queries.Customers
{
    internal class CustomersAllQueryHandler : IRequestHandler<CustomersAllQuery, IEnumerable<CustomerViewModel>>
    {
        public async Task<IEnumerable<CustomerViewModel>> Handle(CustomersAllQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new List<CustomerViewModel>());
        }
    }
}
