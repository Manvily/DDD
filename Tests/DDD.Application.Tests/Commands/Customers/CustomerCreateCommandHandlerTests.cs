using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Customers;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;
using Shared.Domain.Events;
using Xunit;

namespace DDD.Application.Tests.Commands.Customers;

public class CustomerCreateCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICustomersCommandRepository> _repositoryMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CustomerCreateCommandHandler _handler;

    public CustomerCreateCommandHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<ICustomersCommandRepository>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CustomerCreateCommandHandler(_mapperMock.Object, _repositoryMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomerAndReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var command = new CustomerCreateCommand
        {
            Name = new CustomerNameDto { First = "Jan", Last = "Kowalski" },
            Contact = new ContactDto { Email = "jan@example.com", Phone = "123456789" },
            Address = new AddressDto { Street = "Testowa 1", City = "Warszawa", ZipCode = "00-001", Country = "Polska" },
            BirthDate = new DateTime(1990, 1, 1)
        };

        var customer = new Customer(
            new CustomerName("Jan", "Kowalski"),
            new Contact("jan@example.com", "123456789"),
            new Address("Testowa 1", "Warszawa", "00-001", "Polska"),
            new DateTime(1990, 1, 1)
        ) { Id = Guid.NewGuid() };
        var customerDto = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Contact = command.Contact,
            Address = command.Address,
            BirthDate = command.BirthDate
        };

        _mapperMock.Setup(x => x.Map<Customer>(command)).Returns(customer);
        _mapperMock.Setup(x => x.Map<CustomerDto>(customer)).Returns(customerDto);
        _repositoryMock.Setup(x => x.Create(customer)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(customerDto);
        
        _repositoryMock.Verify(x => x.Create(customer), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<CustomerCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFailsToCreate()
    {
        // Arrange
        var command = new CustomerCreateCommand
        {
            Name = new CustomerNameDto { First = "Jan", Last = "Kowalski" },
            Contact = new ContactDto { Email = "jan@example.com", Phone = "123456789" },
            Address = new AddressDto { Street = "Testowa 1", City = "Warszawa", ZipCode = "00-001", Country = "Polska" },
            BirthDate = new DateTime(1990, 1, 1)
        };

        var customer = new Customer(
            new DDD.Domain.ValueObjects.CustomerName("Jan", "Kowalski"),
            new DDD.Domain.ValueObjects.Contact("jan@example.com", "123456789"),
            new DDD.Domain.ValueObjects.Address("Testowa 1", "Warszawa", "00-001", "Polska"),
            new DateTime(1990, 1, 1)
        ) { Id = Guid.NewGuid() };

        _mapperMock.Setup(x => x.Map<Customer>(command)).Returns(customer);
        _repositoryMock.Setup(x => x.Create(customer)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Could not create customer");
        
        _repositoryMock.Verify(x => x.Create(customer), Times.Once);
        _mediatorMock.Verify(x => x.Publish(It.IsAny<CustomerCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
