using Analytics.Application.Abstractions;
using Analytics.Application.Commands;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Events;
using Xunit;

namespace Analytics.Tests.Application.Commands;

public class StoreAnalyticsEventCommandHandlerTests
{
    private readonly IAnalyticsEventCommandRepository _repositoryMock;
    private readonly ILogger<StoreAnalyticsEventCommandHandler> _loggerMock;
    private readonly StoreAnalyticsEventCommandHandler _handler;

    public StoreAnalyticsEventCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IAnalyticsEventCommandRepository>();
        _loggerMock = Substitute.For<ILogger<StoreAnalyticsEventCommandHandler>>();
        _handler = new StoreAnalyticsEventCommandHandler(_repositoryMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldStoreEventSuccessfully_WhenEventIsValid()
    {
        // Arrange
        var domainEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var command = new StoreAnalyticsEventCommand(domainEvent);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _repositoryMock.Received(1).StoreEventAsync(domainEvent, Arg.Any<TimeSpan>(), null);
    }

    [Fact]
    public async Task Handle_ShouldStoreFailedEventAndRethrow_WhenRepositoryThrows()
    {
        // Arrange
        var domainEvent = new CustomerCreatedEvent(Guid.NewGuid(), "Jan Kowalski", "MainApi");
        var command = new StoreAnalyticsEventCommand(domainEvent);
        var expectedException = new Exception("Database connection failed");

        // First call throws, second call for storing failed event succeeds
        _repositoryMock.StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
                      .Returns(Task.FromException(expectedException), Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);

        // Verify both calls were made
        await _repositoryMock.Received(2).StoreEventAsync(Arg.Any<Shared.Domain.Core.IDomainEvent>(), Arg.Any<TimeSpan>(), Arg.Any<string?>());
    }
}