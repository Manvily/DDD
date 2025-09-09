using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Orders;
using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Products;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Shared.Domain.Events;

namespace DDD.Application.Tests.Commands.Orders;

public class OrderCreateCommandHandlerTests
{
    private readonly IMapper _mapper;
    private readonly IOrderCommandRepository _orderRepository;
    private readonly ICustomersCommandRepository _customerRepository;
    private readonly IProductsCommandRepository _productRepository;
    private readonly IMediator _mediator;
    private readonly OrderCreateCommandHandler _handler;

    public OrderCreateCommandHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _orderRepository = Substitute.For<IOrderCommandRepository>();
        _customerRepository = Substitute.For<ICustomersCommandRepository>();
        _productRepository = Substitute.For<IProductsCommandRepository>();
        _mediator = Substitute.For<IMediator>();
        
        _handler = new OrderCreateCommandHandler(
            _mapper,
            _mediator,
            _orderRepository,
            _productRepository,
            _customerRepository);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderAndReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = productIds
        };

        var customer = new Customer(
            new DDD.Domain.ValueObjects.CustomerName("Test", "Customer"),
            new DDD.Domain.ValueObjects.Contact("test@example.com", "123456789"),
            new DDD.Domain.ValueObjects.Address("Test St", "Test City", "12345", "Test Country"),
            DateTime.UtcNow.AddYears(-30)
        ) { Id = customerId };
        
        var products = new List<Product> 
        { 
            new Product(
                new DDD.Domain.ValueObjects.NameValue("Product 1"),
                new DDD.Domain.ValueObjects.Price(10.99m),
                new Category(new DDD.Domain.ValueObjects.NameValue("Category 1"))
            ),
            new Product(
                new DDD.Domain.ValueObjects.NameValue("Product 2"),
                new DDD.Domain.ValueObjects.Price(20.99m),
                new Category(new DDD.Domain.ValueObjects.NameValue("Category 2"))
            )
        };
        
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Customer = new CustomerDto
            {
                Id = customerId,
                Name = new CustomerNameDto { First = "Test", Last = "Customer" },
                Contact = new ContactDto { Email = "test@example.com", Phone = "123456789" },
                Address = new AddressDto { Street = "Test St", City = "Test City", ZipCode = "12345", Country = "Test Country" },
                BirthDate = DateTime.UtcNow.AddYears(-30)
            },
            OrderDate = DateTime.UtcNow,
            Products = new List<ProductDto>(),
            Payment = new PaymentStatusDto { IsPaid = false }
        };

        _customerRepository.FindAsync(customerId).Returns(customer);
        _productRepository.FindMany(productIds).Returns(products);
        _orderRepository.CreateOrder(Arg.Do<Order>(o => o.Id = Guid.NewGuid())).Returns(true);
        _mapper.Map<OrderDto>(Arg.Any<Order>()).Returns(orderDto);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(orderDto);
        
        await _customerRepository.Received(1).FindAsync(customerId);
        await _productRepository.Received(1).FindMany(productIds);
        await _orderRepository.Received(1).CreateOrder(Arg.Any<Order>());
        await _mediator.Received(1).Publish(Arg.Any<OrderCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = new[] { Guid.NewGuid() }
        };

        _customerRepository.FindAsync(customerId).Returns((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Customer not found");
        
        await _customerRepository.Received(1).FindAsync(customerId);
        await _productRepository.DidNotReceive().FindMany(Arg.Any<IEnumerable<Guid>>());
        await _orderRepository.DidNotReceive().CreateOrder(Arg.Any<Order>());
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = productIds
        };

        var customer = new Customer(
            new DDD.Domain.ValueObjects.CustomerName("Test", "Customer"),
            new DDD.Domain.ValueObjects.Contact("test@example.com", "123456789"),
            new DDD.Domain.ValueObjects.Address("Test St", "Test City", "12345", "Test Country"),
            DateTime.UtcNow.AddYears(-30)
        ) { Id = Guid.NewGuid() };
        var products = new List<Product>(); // Empty list

        _customerRepository.FindAsync(customerId).Returns(customer);
        _productRepository.FindMany(productIds).Returns(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Products not found");
        
        await _customerRepository.Received(1).FindAsync(customerId);
        await _productRepository.Received(1).FindMany(productIds);
        await _orderRepository.DidNotReceive().CreateOrder(Arg.Any<Order>());
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSomeProductsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = productIds
        };

        var customer = new Customer(
            new DDD.Domain.ValueObjects.CustomerName("Test", "Customer"),
            new DDD.Domain.ValueObjects.Contact("test@example.com", "123456789"),
            new DDD.Domain.ValueObjects.Address("Test St", "Test City", "12345", "Test Country"),
            DateTime.UtcNow.AddYears(-30)
        ) { Id = Guid.NewGuid() };
        var products = new List<Product> 
        { 
            new Product(
                new DDD.Domain.ValueObjects.NameValue("Product 1"),
                new DDD.Domain.ValueObjects.Price(10.99m),
                new Category(new DDD.Domain.ValueObjects.NameValue("Category 1"))
            )
        }; // Only one product found

        _customerRepository.FindAsync(customerId).Returns(customer);
        _productRepository.FindMany(productIds).Returns(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Some products were not found");
        
        await _customerRepository.Received(1).FindAsync(customerId);
        await _productRepository.Received(1).FindMany(productIds);
        await _orderRepository.DidNotReceive().CreateOrder(Arg.Any<Order>());
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenOrderCreationFails()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid() };
        
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = productIds
        };

        var customer = new Customer(
            new DDD.Domain.ValueObjects.CustomerName("Test", "Customer"),
            new DDD.Domain.ValueObjects.Contact("test@example.com", "123456789"),
            new DDD.Domain.ValueObjects.Address("Test St", "Test City", "12345", "Test Country"),
            DateTime.UtcNow.AddYears(-30)
        ) { Id = customerId };
        
        var products = new List<Product> 
        { 
            new Product(
                new DDD.Domain.ValueObjects.NameValue("Product 1"),
                new DDD.Domain.ValueObjects.Price(10.99m),
                new Category(new DDD.Domain.ValueObjects.NameValue("Category 1"))
            )
        };

        _customerRepository.FindAsync(customerId).Returns(customer);
        _productRepository.FindMany(productIds).Returns(products);
        _orderRepository.CreateOrder(Arg.Any<Order>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Could not create order");
        
        await _customerRepository.Received(1).FindAsync(customerId);
        await _productRepository.Received(1).FindMany(productIds);
        await _orderRepository.Received(1).CreateOrder(Arg.Any<Order>());
        await _mediator.DidNotReceive().Publish(Arg.Any<OrderCreatedEvent>(), Arg.Any<CancellationToken>());
    }
}