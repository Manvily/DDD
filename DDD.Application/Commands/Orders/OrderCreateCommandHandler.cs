using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Customers;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDD.Application.Commands.Orders
{
    internal class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, OrderDto>
    {
        private readonly IMapper _mapper;
        private readonly IOrderCommandRepository _orderCommandRepository;
        private readonly ICustomersCommandRepository _customersCommandRepository;
        private readonly IProductsCommandRepository _productsCommandRepository;

        public OrderCreateCommandHandler(
            IMapper mapper, 
            IOrderCommandRepository orderCommandRepository,
            IProductsCommandRepository productsCommandRepository,
            ICustomersCommandRepository customersCommandRepository)
        {
            _mapper = mapper;
            _orderCommandRepository = orderCommandRepository;
            _customersCommandRepository = customersCommandRepository;
            _productsCommandRepository = productsCommandRepository;
        }

        public async Task<OrderDto> Handle(OrderCreateCommand command, CancellationToken cancellationToken)
        {
            var customer = await _customersCommandRepository.Find(command.CustomerId);

            if (customer == null)
                throw new Exception("Customer not found");

            var products = await _productsCommandRepository.FindMany(command.ProductsIds);


            if (products == null || products.Count() == 0)
                throw new Exception("Products not found");

            if (products.Count() != command.ProductsIds.Count())
                throw new Exception("Some products were not found");

            var entity = new Order(customer, new DateTimeOffset(), products, new PaymentStatus(false));
            var created = await _orderCommandRepository.CreateOrder(entity);

            if (created == false)
                throw new Exception("Could not create order");

            var dto = _mapper.Map<OrderDto>(entity);
            return dto;
        }
    }
}