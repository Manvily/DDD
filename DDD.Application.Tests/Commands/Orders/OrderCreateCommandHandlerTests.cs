using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Orders;
using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Products;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Events;
using Xunit;

namespace DDD.Application.Tests.Commands.Orders;

public class OrderCreateCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IOrderCommandRepository> _orderRepositoryMock;
    private readonly Mock<ICustomersCommandRepository> _customerRepositoryMock;
    private readonly Mock<IProductsCommandRepository> _productRepositoryMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrderCreateCommandHandler _handler;

    public OrderCreateCommandHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _orderRepositoryMock = new Mock<IOrderCommandRepository>();
        _customerRepositoryMock = new Mock<ICustomersCommandRepository>();
        _productRepositoryMock = new Mock<IProductsCommandRepository>();
        _mediatorMock = new Mock<IMediator>();
        
        _handler = new OrderCreateCommandHandler(
            _mapperMock.Object,
            _mediatorMock.Object,
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _customerRepositoryMock.Object);
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
        var order = new Order(customer, DateTime.UtcNow, products, new DDD.Domain.ValueObjects.PaymentStatus(false)) { Id = orderId };
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

        _customerRepositoryMock.Setup(x => x.FindAsync(customerId)).ReturnsAsync(customer);
        _productRepositoryMock.Setup(x => x.FindMany(productIds)).ReturnsAsync(products);
        _orderRepositoryMock.Setup(x => x.CreateOrder(It.IsAny<Order>()))
                         .Callback<Order>(o => o.Id = orderId)  // Set ID when order is created
                         .ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<OrderDto>(It.IsAny<Order>())).Returns(orderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(orderDto);
        
        _customerRepositoryMock.Verify(x => x.FindAsync(customerId), Times.Once);
        _productRepositoryMock.Verify(x => x.FindMany(productIds), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
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

        _customerRepositoryMock.Setup(x => x.FindAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Customer not found");
        
        _customerRepositoryMock.Verify(x => x.FindAsync(customerId), Times.Once);
        _productRepositoryMock.Verify(x => x.FindMany(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
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

        _customerRepositoryMock.Setup(x => x.FindAsync(customerId)).ReturnsAsync(customer);
        _productRepositoryMock.Setup(x => x.FindMany(productIds)).ReturnsAsync(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Products not found");
        
        _customerRepositoryMock.Verify(x => x.FindAsync(customerId), Times.Once);
        _productRepositoryMock.Verify(x => x.FindMany(productIds), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
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

        _customerRepositoryMock.Setup(x => x.FindAsync(customerId)).ReturnsAsync(customer);
        _productRepositoryMock.Setup(x => x.FindMany(productIds)).ReturnsAsync(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Some products were not found");
        
        _customerRepositoryMock.Verify(x => x.FindAsync(customerId), Times.Once);
        _productRepositoryMock.Verify(x => x.FindMany(productIds), Times.Once);
        _orderRepositoryMock.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
    }
}
