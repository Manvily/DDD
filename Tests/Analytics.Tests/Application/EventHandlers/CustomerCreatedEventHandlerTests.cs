using Analytics.Application.Commands;
using Analytics.Application.EventHandlers.Processors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Events;
using Xunit;

namespace Analytics.Tests.Application.EventHandlers;

public class CustomerCreatedEventHandlerTests
{
    private readonly IMediator _mediatorMock;
    private readonly ILogger<CustomerCreatedEventHandler> _loggerMock;
    private readonly CustomerCreatedEventHandler _handler;

    public CustomerCreatedEventHandlerTests()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _loggerMock = Substitute.For<ILogger<CustomerCreatedEventHandler>>();
        _handler = new CustomerCreatedEventHandler(_mediatorMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldSendStoreAnalyticsEventCommand_WhenEventIsReceived()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "Jan Kowalski";
        var serviceName = "MainApi";
        
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, serviceName);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        await _mediatorMock.Received(1).Send(
            Arg.Is<StoreAnalyticsEventCommand>(cmd => cmd.DomainEvent == customerCreatedEvent),
            Arg.Any<CancellationToken>());

        // Verify logging
        VerifyLogCalled(LogLevel.Information, $"Processing CustomerCreatedEvent: {customerCreatedEvent.EventId}");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenMediatorThrows()
    {
        // Arrange
        var customerCreatedEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var expectedException = new Exception("Command processing failed");

        _mediatorMock.Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromException<Unit>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(customerCreatedEvent, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);

        await _mediatorMock.Received(1).Send(Arg.Any<StoreAnalyticsEventCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectCommand_WithEventData()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "Jan Kowalski 2";
        var serviceName = "TestService";
        
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, serviceName);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert - verify the correct command was sent
        await _mediatorMock.Received(1).Send(
            Arg.Is<StoreAnalyticsEventCommand>(cmd => cmd.DomainEvent == customerCreatedEvent),
            Arg.Any<CancellationToken>());
    }

    private void VerifyLogCalled(LogLevel level, string messageContains)
    {
        _loggerMock.Received().Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains(messageContains)),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
