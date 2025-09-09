using Analytics.Application.Commands;
using Analytics.Application.EventHandlers.Processors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Events;
using Xunit;

namespace Analytics.Tests.Application.EventHandlers;

public class CustomerCreatedEventHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CustomerCreatedEventHandler>> _loggerMock;
    private readonly CustomerCreatedEventHandler _handler;

    public CustomerCreatedEventHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CustomerCreatedEventHandler>>();
        _handler = new CustomerCreatedEventHandler(_mediatorMock.Object, _loggerMock.Object);
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
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<StoreAnalyticsEventCommand>(cmd => cmd.DomainEvent == customerCreatedEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify logging
        VerifyLogCalled(LogLevel.Information, $"Processing CustomerCreatedEvent: {customerCreatedEvent.EventId}");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenMediatorThrows()
    {
        // Arrange
        var customerCreatedEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var expectedException = new Exception("Command processing failed");

        _mediatorMock.Setup(x => x.Send(It.IsAny<StoreAnalyticsEventCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(customerCreatedEvent, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);

        _mediatorMock.Verify(
            x => x.Send(It.IsAny<StoreAnalyticsEventCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectCommand_WithEventData()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "Jan Kowalski 2";
        var serviceName = "TestService";
        
        var customerCreatedEvent = new CustomerCreatedEvent(customerId, customerName, serviceName);
        StoreAnalyticsEventCommand? capturedCommand = null;

        _mediatorMock.Setup(x => x.Send(It.IsAny<StoreAnalyticsEventCommand>(), It.IsAny<CancellationToken>()))
                    .Callback<IRequest<Unit>, CancellationToken>((cmd, ct) => capturedCommand = cmd as StoreAnalyticsEventCommand)
                    .ReturnsAsync(Unit.Value);

        // Act
        await _handler.Handle(customerCreatedEvent, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.DomainEvent.Should().BeEquivalentTo(customerCreatedEvent);
    }

    private void VerifyLogCalled(LogLevel level, string messageContains)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
