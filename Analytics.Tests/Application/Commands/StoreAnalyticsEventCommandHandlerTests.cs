using Analytics.Application.Abstractions;
using Analytics.Application.Commands;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Events;
using Xunit;

namespace Analytics.Tests.Application.Commands;

public class StoreAnalyticsEventCommandHandlerTests
{
    private readonly Mock<IAnalyticsEventCommandRepository> _repositoryMock;
    private readonly Mock<ILogger<StoreAnalyticsEventCommandHandler>> _loggerMock;
    private readonly StoreAnalyticsEventCommandHandler _handler;

    public StoreAnalyticsEventCommandHandlerTests()
    {
        _repositoryMock = new Mock<IAnalyticsEventCommandRepository>();
        _loggerMock = new Mock<ILogger<StoreAnalyticsEventCommandHandler>>();
        _handler = new StoreAnalyticsEventCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldStoreEventSuccessfully_WhenEventIsValid()
    {
        // Arrange
        var domainEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var command = new StoreAnalyticsEventCommand(domainEvent);

        _repositoryMock.Setup(x => x.StoreEventAsync(domainEvent, It.IsAny<TimeSpan>(), null))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _repositoryMock.Verify(x => x.StoreEventAsync(domainEvent, It.IsAny<TimeSpan>(), null), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStoreFailedEventAndRethrow_WhenRepositoryThrows()
    {
        // Arrange
        var domainEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var command = new StoreAnalyticsEventCommand(domainEvent);
        var expectedException = new Exception("Database connection failed");

        // First call throws, second call for storing failed event succeeds
        _repositoryMock.SetupSequence(x => x.StoreEventAsync(It.IsAny<Shared.Domain.Core.IDomainEvent>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
                      .ThrowsAsync(expectedException)
                      .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);

        // Verify both calls were made
        _repositoryMock.Verify(x => x.StoreEventAsync(It.IsAny<Shared.Domain.Core.IDomainEvent>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Exactly(2));
    }
}