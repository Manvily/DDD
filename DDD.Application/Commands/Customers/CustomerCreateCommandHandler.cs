using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using MediatR;
using Shared.Domain.Events;

namespace DDD.Application.Commands.Customers
{
    internal class CustomerCreateCommandHandler(IMapper mapper, ICustomersCommandRepository customersCommandRepository,
            IMediator mediator)
        : IRequestHandler<CustomerCreateCommand, CustomerDto>
    {
        public async Task<CustomerDto> Handle(CustomerCreateCommand command, CancellationToken cancellationToken)
        {
            var entity = mapper.Map<Customer>(command);
            var created = await customersCommandRepository.Create(entity);

            if (created == false)
                throw new Exception("Could not create customer");

            var dto = mapper.Map<CustomerDto>(entity);

            _ = mediator.Publish(CreateEvent(entity), cancellationToken);
            return dto;
        }
        
        private static CustomerCreatedEvent CreateEvent(Customer customer)
        {
            return new CustomerCreatedEvent(customer.Id, customer.Name.ToString() ?? string.Empty, "MainApi");
        }
    }
}