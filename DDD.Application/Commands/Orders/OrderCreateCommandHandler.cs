using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Domain.Entities;
using DDD.Domain.Events;
using DDD.Domain.ValueObjects;
using MediatR;

namespace DDD.Application.Commands.Orders
{
    internal class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, OrderDto>
    {
        private readonly IMapper _mapper;
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly ICustomersCommandRepository _customersCommandRepository;
        private readonly IProductsCommandRepository _productsCommandRepository;
        private readonly IMediator _mediator;

        public OrderCreateCommandHandler(
            IMapper mapper, 
            IMediator mediator,
            IOrderCommandRepository orderCommandRepository,
            IProductsCommandRepository productsCommandRepository,
            ICustomersCommandRepository customersCommandRepository)
        {
            _mapper = mapper;
            _orderCommandRepository = orderCommandRepository;
            _customersCommandRepository = customersCommandRepository;
            _productsCommandRepository = productsCommandRepository;
            _mediator = mediator;
        }

        public async Task<OrderDto> Handle(OrderCreateCommand command, CancellationToken cancellationToken)
        {
            var customer = await _customersCommandRepository.FindAsync(command.CustomerId);

            if (customer == null)
                throw new Exception("Customer not found");

            var products = await _productsCommandRepository.FindMany(command.ProductsIds);


            if (products == null || !products.Any())
                throw new Exception("Products not found");

            if (products.Count() != command.ProductsIds.Count())
                throw new Exception("Some products were not found");

            var entity = new Order(customer, new DateTime(), products, new PaymentStatus(false));
            var created = await _orderCommandRepository.CreateOrder(entity);

            if (created == false)
                throw new Exception("Could not create order");

            _ = _mediator.Publish(CreateEvent(entity), cancellationToken);
            
            var dto = _mapper.Map<OrderDto>(entity);
            return dto;
        }
        
        private OrderAdded CreateEvent(Order order)
        {
            return new OrderAdded
            {
                CustomerId = order.Customer.Id
            };
        }
    }
}