using DDD.Api.Queries.Controllers;
using DDD.Application.Queries.Customers;
using DDD.Application.Mapper.Dtos;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DDD.Api.Tests.Controllers;

public class CustomersQueryControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CustomersQueryController>> _loggerMock;
    private readonly CustomersQueryController _controller;

    public CustomersQueryControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CustomersQueryController>>();
        _controller = new CustomersQueryController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomersList_ShouldReturnOkResult_WithCustomersList()
    {
        // Arrange
        var expectedCustomers = new List<CustomerViewModel>
        {
            new CustomerViewModel
            {
                Id = Guid.NewGuid(),
                Name = new CustomerNameDto { First = "Jan", Last = "Kowalski" },
                Contact = new ContactDto { Email = "jan@example.com", Phone = "123456789" },
                Address = new AddressDto { Street = "Testowa 1", City = "Warszawa", ZipCode = "00-001", Country = "Polska" },
                BirthDate = new DateTime(1990, 1, 1)
            },
            new CustomerViewModel
            {
                Id = Guid.NewGuid(),
                Name = new CustomerNameDto { First = "Anna", Last = "Nowak" },
                Contact = new ContactDto { Email = "anna@example.com", Phone = "987654321" },
                Address = new AddressDto { Street = "Główna 5", City = "Kraków", ZipCode = "30-001", Country = "Polska" },
                BirthDate = new DateTime(1985, 5, 15)
            }
        };

        _mediatorMock.Setup(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _controller.GetCustomersList();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<CustomerViewModel>>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedCustomers);
        
        _mediatorMock.Verify(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersList_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        var emptyList = new List<CustomerViewModel>();
        
        _mediatorMock.Setup(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetCustomersList();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<CustomerViewModel>>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(emptyList);
        
        _mediatorMock.Verify(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersList_ShouldPropagateException_WhenMediatorThrows()
    {
        // Arrange
        var expectedException = new Exception("Database connection failed");
        _mediatorMock.Setup(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _controller.GetCustomersList());
        exception.Should().BeEquivalentTo(expectedException);
        
        _mediatorMock.Verify(x => x.Send(It.IsAny<CustomersAllQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
