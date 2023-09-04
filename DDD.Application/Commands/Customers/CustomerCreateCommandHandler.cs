using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Domain.Events;
using MediatR;

namespace DDD.Application.Commands.Customers
{
    internal class CustomerCreateCommandHandler : IRequestHandler<CustomerCreateCommand, CustomerDto>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICustomersCommandRepository _customersCommandRepository;

        public CustomerCreateCommandHandler(IMapper mapper, ICustomersCommandRepository customersCommandRepository,
            IMediator mediator)
        {
            _mapper = mapper;
            _customersCommandRepository = customersCommandRepository;
            _mediator = mediator;
        }

        public async Task<CustomerDto> Handle(CustomerCreateCommand command, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Customer>(command);
            var created = await _customersCommandRepository.Create(entity);

            if (created == false)
                throw new Exception("Could not create customer");

            var dto = _mapper.Map<CustomerDto>(entity);
            
            _ =_mediator.Publish(new CustomerAdded(), cancellationToken);
            return dto;
        }
    }
}