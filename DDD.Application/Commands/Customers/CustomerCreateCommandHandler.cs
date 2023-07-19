using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Queries.Customers;
using DDD.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Customers
{
    internal class CustomerCreateCommandHandler : IRequestHandler<CustomerCreateCommand, CustomerDto>
    {
        private readonly IMapper _mapper;
        private readonly ICustomersCommandRepository _customersCommandRepository;

        public CustomerCreateCommandHandler(IMapper mapper, ICustomersCommandRepository customersCommandRepository)
        {
            _mapper = mapper;
            _customersCommandRepository = customersCommandRepository;
        }

        public async Task<CustomerDto> Handle(CustomerCreateCommand command, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Customer>(command);
            var created = await _customersCommandRepository.Create(entity);

            if (created == false)
                throw new Exception("Could not create customer");

            var dto = _mapper.Map<CustomerDto>(entity);

            return dto;
        }
    }
}
