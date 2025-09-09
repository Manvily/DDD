using AutoMapper;
using DDD.Application.Abstractions;
using DDD.Application.Commands.Customers;
using DDD.Application.Mapper.Dtos;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Shared.Domain.Events;

namespace DDD.Application.Tests.Commands.Customers;

public class CustomerCreateCommandHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ICustomersCommandRepository _repository;
    private readonly IMediator _mediator;
    private readonly CustomerCreateCommandHandler _handler;

    public CustomerCreateCommandHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _repository = Substitute.For<ICustomersCommandRepository>();
        _mediator = Substitute.For<IMediator>();
        _handler = new CustomerCreateCommandHandler(_mapper, _repository, _mediator);
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

        _mapper.Map<Customer>(command).Returns(customer);
        _mapper.Map<CustomerDto>(customer).Returns(customerDto);
        _repository.Create(customer).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(customerDto);
        
        await _repository.Received(1).Create(customer);
        await _mediator.Received(1).Publish(Arg.Any<CustomerCreatedEvent>(), Arg.Any<CancellationToken>());
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

        _mapper.Map<Customer>(command).Returns(customer);
        _repository.Create(customer).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Could not create customer");
        
        await _repository.Received(1).Create(customer);
        await _mediator.DidNotReceive().Publish(Arg.Any<CustomerCreatedEvent>(), Arg.Any<CancellationToken>());
    }
}
