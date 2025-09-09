using Analytics.Application.Abstractions;
using Analytics.Application.Commands;
using AutoFixture;
using Bogus;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Exceptions;
using Shared.Domain.Events;

namespace Analytics.Tests.Commands;

public class StoreAnalyticsEventCommandHandlerTests
{
    private readonly IAnalyticsEventCommandRepository _commandRepository;
    private readonly ILogger<StoreAnalyticsEventCommandHandler> _logger;
    private readonly StoreAnalyticsEventCommandHandler _handler;
    private readonly IFixture _fixture;
    private readonly Faker _faker;

    public StoreAnalyticsEventCommandHandlerTests()
    {
        _commandRepository = Substitute.For<IAnalyticsEventCommandRepository>();
        _logger = Substitute.For<ILogger<StoreAnalyticsEventCommandHandler>>();
        _handler = new StoreAnalyticsEventCommandHandler(_commandRepository, _logger);
        _fixture = new Fixture();
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_Should_StoreEventAndReturnUnit_When_CommandIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);

        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _commandRepository.Received(1).StoreEventAsync(
            Arg.Is<Shared.Domain.Core.IDomainEvent>(e => e == domainEvent),
            Arg.Any<TimeSpan>(),
            Arg.Is<string?>(s => s == null));
    }

    [Fact]
    public async Task Handle_Should_LogInformationMessages_When_ProcessingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);

        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Storing analytics event: {domainEvent.EventType}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Successfully stored {domainEvent.EventType} event")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_Should_MeasureProcessingTime_When_StoringEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);

        TimeSpan capturedElapsed = TimeSpan.Zero;
        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => capturedElapsed = callInfo.Arg<TimeSpan>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedElapsed.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_Should_LogErrorAndStoreFailedEvent_When_RepositoryThrows()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);
        var expectedException = new InvalidOperationException("Repository failed");

        _commandRepository.StoreEventAsync(
                Arg.Any<Shared.Domain.Core.IDomainEvent>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Is<string?>(s => s == null))
            .Throws(expectedException);

        _commandRepository.StoreEventAsync(
                Arg.Any<Shared.Domain.Core.IDomainEvent>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Is<string?>(s => s != null))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(expectedException);

        // Verify error logging
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Failed to store {domainEvent.EventType} event")),
            Arg.Is<Exception>(ex => ex == expectedException),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify failed event storage attempt
        await _commandRepository.Received(1).StoreEventAsync(
            Arg.Is<Shared.Domain.Core.IDomainEvent>(e => e == domainEvent),
            Arg.Any<TimeSpan>(),
            Arg.Is<string?>(s => s == expectedException.Message));
    }

    [Fact]
    public async Task Handle_Should_LogErrorForFailedEventStorage_When_BothStorageAttemptsThrow()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);
        var primaryException = new InvalidOperationException("Primary storage failed");
        var retryException = new InvalidOperationException("Retry storage failed");

        _commandRepository.StoreEventAsync(
                Arg.Any<Shared.Domain.Core.IDomainEvent>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Is<string?>(s => s == null))
            .Throws(primaryException);

        _commandRepository.StoreEventAsync(
                Arg.Any<Shared.Domain.Core.IDomainEvent>(), 
                Arg.Any<TimeSpan>(), 
                Arg.Is<string?>(s => s != null))
            .Throws(retryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(primaryException);

        // Verify both error logs
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Failed to store {domainEvent.EventType} event")),
            Arg.Is<Exception>(ex => ex == primaryException),
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Failed to store failed event {domainEvent.EventType}")),
            Arg.Is<Exception>(ex => ex == retryException),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_Should_HandleCancellation_When_CancellationTokenIsCancelled()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(callInfo => 
            {
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Task.CompletedTask;
            });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNotFoundException>(() =>
            _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_Should_WorkWithDifferentEventTypes_When_ProcessingVariousEvents()
    {
        // Arrange
        var customerEvent = new CustomerCreatedEvent(Guid.NewGuid(), "John Doe", "Source1");
        var orderEvent = new OrderCreatedEvent(Guid.NewGuid(), "Source2", Guid.NewGuid());

        var customerCommand = new StoreAnalyticsEventCommand(customerEvent);
        var orderCommand = new StoreAnalyticsEventCommand(orderEvent);

        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask);

        // Act
        var customerResult = await _handler.Handle(customerCommand, CancellationToken.None);
        var orderResult = await _handler.Handle(orderCommand, CancellationToken.None);

        // Assert
        customerResult.Should().Be(Unit.Value);
        orderResult.Should().Be(Unit.Value);

        await _commandRepository.Received(1).StoreEventAsync(
            Arg.Is<Shared.Domain.Core.IDomainEvent>(e => e == customerEvent),
            Arg.Any<TimeSpan>(),
            Arg.Is<string?>(s => s == null));

        await _commandRepository.Received(1).StoreEventAsync(
            Arg.Is<Shared.Domain.Core.IDomainEvent>(e => e == orderEvent),
            Arg.Any<TimeSpan>(),
            Arg.Is<string?>(s => s == null));
    }

    [Fact]
    public async Task Handle_Should_PassCorrectEventProperties_When_StoringEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "Test Customer";
        var source = "TestSource";
        var domainEvent = new CustomerCreatedEvent(customerId, customerName, source);
        var command = new StoreAnalyticsEventCommand(domainEvent);

        Shared.Domain.Core.IDomainEvent? capturedEvent = null;
        _commandRepository.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo => capturedEvent = callInfo.Arg<Shared.Domain.Core.IDomainEvent>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.Should().Be(domainEvent);
        capturedEvent.EventId.Should().Be(domainEvent.EventId);
        capturedEvent.AggregateId.Should().Be(customerId);
        capturedEvent.EventType.Should().Be("CustomerCreatedEvent");
        capturedEvent.Source.Should().Be(source);
    }
}
