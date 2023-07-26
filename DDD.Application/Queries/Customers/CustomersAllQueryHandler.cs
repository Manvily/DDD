using AutoMapper;
using DDD.Application.Abstractions;
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
        private readonly ICustomersQueryRepository _customersQueryRepository;
        private readonly IMapper _mapper;
        public CustomersAllQueryHandler(ICustomersQueryRepository customersQueryRepository, IMapper mapper)
        {
            _customersQueryRepository = customersQueryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerViewModel>> Handle(CustomersAllQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customersQueryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerViewModel>>(customers);
        }
    }
}
