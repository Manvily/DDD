using Analytics.Application.Commands;
using Analytics.Application.EventHandlers.Processors;
using AutoFixture;
using Bogus;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shared.Domain.Events;

namespace Analytics.Tests.EventHandlers;

public class CustomerCreatedEventHandlerTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomerCreatedEventHandler> _logger;
    private readonly CustomerCreatedEventHandler _handler;
    private readonly IFixture _fixture;
    private readonly Faker _faker;

    public CustomerCreatedEventHandlerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<CustomerCreatedEventHandler>>();
        _handler = new CustomerCreatedEventHandler(_mediator, _logger);
        _fixture = new Fixture();
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_Should_ProcessEventAndSendCommand_When_EventIsReceived()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<StoreAnalyticsEventCommand>(cmd => cmd.DomainEvent == customerCreatedEvent),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_LogInformation_When_ProcessingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Processing CustomerCreatedEvent: {customerCreatedEvent.EventId}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_Should_PassCorrectEventToCommand_When_Processing()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "John Doe";
        var source = "TestSource";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        StoreAnalyticsEventCommand? capturedCommand = null;
        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value)
            .AndDoes(callInfo => capturedCommand = callInfo.Arg<StoreAnalyticsEventCommand>());

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.DomainEvent.Should().Be(customerCreatedEvent);
        capturedCommand.DomainEvent.EventId.Should().Be(customerCreatedEvent.EventId);
        capturedCommand.DomainEvent.AggregateId.Should().Be(customerId);
        capturedCommand.DomainEvent.EventType.Should().Be("CustomerCreatedEvent");
    }

    [Fact]
    public async Task Handle_Should_PropagateException_When_MediatorThrows()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var expectedException = new InvalidOperationException("Mediator failed");

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(customerCreatedEvent, CancellationToken.None));

        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_Should_HandleCancellation_When_CancellationTokenIsCancelled()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            {
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Unit.Value;
            });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(customerCreatedEvent, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken_When_CallingMediator()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        // Act
        await _handler.Handle(customerCreatedEvent, cancellationToken);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<StoreAnalyticsEventCommand>(),
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }

    [Fact]
    public async Task Handle_Should_WorkWithDifferentCustomerNames_When_ProcessingMultipleEvents()
    {
        // Arrange
        var events = new[]
        {
            new CustomerCreatedEvent(Guid.NewGuid(), "John Doe", "Source1"),
            new CustomerCreatedEvent(Guid.NewGuid(), "Jane Smith", "Source2"),
            new CustomerCreatedEvent(Guid.NewGuid(), "Bob Johnson", "Source3")
        };

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        // Act
        foreach (var customerEvent in events)
        {
            await _handler.Handle(customerEvent, CancellationToken.None);
        }

        // Assert
        await _mediator.Received(3).Send(
            Arg.Any<StoreAnalyticsEventCommand>(),
            Arg.Any<CancellationToken>());

        foreach (var customerEvent in events)
        {
            await _mediator.Received(1).Send(
                Arg.Is<StoreAnalyticsEventCommand>(cmd => cmd.DomainEvent == customerEvent),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task Handle_Should_HandleSpecialCharactersInCustomerName_When_EventContainsSpecialChars()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "José María Aznar-López (Jr.) & Co. 中文名字";
        var source = "MainApi";
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, source);

        _mediator.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<StoreAnalyticsEventCommand>(cmd => 
                cmd.DomainEvent == customerCreatedEvent &&
                ((CustomerCreatedEvent)cmd.DomainEvent).CustomerName == customerName),
            Arg.Any<CancellationToken>());
    }
}
