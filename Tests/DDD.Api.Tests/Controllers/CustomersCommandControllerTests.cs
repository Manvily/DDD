using DDD.Api.Commands.Controllers;
using DDD.Application.Commands.Customers;
using DDD.Application.Queries.Customers;
using DDD.Application.Mapper.Dtos;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DDD.Api.Tests.Controllers;

public class CustomersCommandControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CustomersCommandController>> _loggerMock;
    private readonly CustomersCommandController _controller;

    public CustomersCommandControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CustomersCommandController>>();
        _controller = new CustomersCommandController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnOkResult_WhenCommandIsValid()
    {
        // Arrange
        var command = new CustomerCreateCommand
        {
            Name = new CustomerNameDto { First = "Jan", Last = "Kowalski" },
            Contact = new ContactDto { Email = "jan@example.com", Phone = "123456789" },
            Address = new AddressDto { Street = "Testowa 1", City = "Warszawa", ZipCode = "00-001", Country = "Polska" },
            BirthDate = new DateTime(1990, 1, 1)
        };

        var expectedResult = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Contact = command.Contact,
            Address = command.Address,
            BirthDate = command.BirthDate
        };

        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateCustomer(command);

        // Assert
        result.Should().BeOfType<ActionResult<CustomerDto>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResult);
        
        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_ShouldPropagateException_WhenMediatorThrows()
    {
        // Arrange
        var command = new CustomerCreateCommand
        {
            Name = new CustomerNameDto { First = "Jan", Last = "Kowalski" },
            Contact = new ContactDto { Email = "jan@example.com", Phone = "123456789" },
            Address = new AddressDto { Street = "Testowa 1", City = "Warszawa", ZipCode = "00-001", Country = "Polska" },
            BirthDate = new DateTime(1990, 1, 1)
        };

        var expectedException = new Exception("Database error");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _controller.CreateCustomer(command));
        exception.Should().BeEquivalentTo(expectedException);
        
        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
