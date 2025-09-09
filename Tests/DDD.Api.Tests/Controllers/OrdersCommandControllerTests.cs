using DDD.Api.Commands.Controllers;
using DDD.Application.Commands.Orders;
using DDD.Application.Commands.Customers;
using DDD.Application.Commands.Products;
using DDD.Application.Commands.Categories;
using DDD.Application.Mapper.Dtos;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DDD.Api.Tests.Controllers;

public class OrdersCommandControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<OrdersCommandController>> _loggerMock;
    private readonly OrdersCommandController _controller;

    public OrdersCommandControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<OrdersCommandController>>();
        _controller = new OrdersCommandController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnOkResult_WhenCommandIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        
        var command = new OrderCreateCommand
        {
            CustomerId = customerId,
            ProductsIds = productIds
        };

        var expectedResult = new OrderDto
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

        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateOrder(command);

        // Assert
        result.Should().BeOfType<ActionResult<OrderDto>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResult);
        
        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ShouldPropagateException_WhenMediatorThrows()
    {
        // Arrange
        var command = new OrderCreateCommand
        {
            CustomerId = Guid.NewGuid(),
            ProductsIds = new[] { Guid.NewGuid() }
        };

        var expectedException = new Exception("Customer not found");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _controller.CreateOrder(command));
        exception.Should().BeEquivalentTo(expectedException);
        
        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
