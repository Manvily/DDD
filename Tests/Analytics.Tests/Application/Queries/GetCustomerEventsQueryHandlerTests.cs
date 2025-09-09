using Analytics.Application.Abstractions;
using Analytics.Application.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Analytics.Tests.Application.Queries;

public class GetCustomerEventsQueryHandlerTests
{
    private readonly IAnalyticsEventQueryRepository _repositoryMock;
    private readonly ILogger<GetCustomerEventsQueryHandler> _loggerMock;
    private readonly GetCustomerEventsQueryHandler _handler;

    public GetCustomerEventsQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IAnalyticsEventQueryRepository>();
        _loggerMock = Substitute.For<ILogger<GetCustomerEventsQueryHandler>>();
        _handler = new GetCustomerEventsQueryHandler(_repositoryMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerEvents_WhenEventsExist()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        var query = new GetCustomerEventsQuery(fromDate, toDate);

        var expectedEvents = new List<CustomerEventData>
        {
            new CustomerEventData(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "Jan Kowalski",
                DateTime.UtcNow.AddDays(-5),
                "CustomerCreated"
            ),
            new CustomerEventData(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "Jan Kowalski 2",
                DateTime.UtcNow.AddDays(-3),
                "CustomerUpdated"
            )
        };

        _repositoryMock.GetCustomerEventsAsync(fromDate, toDate)
                      .Returns(expectedEvents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedEvents);
        await _repositoryMock.Received(1).GetCustomerEventsAsync(fromDate, toDate);
        
        // Verify logging
        VerifyLogCalled(LogLevel.Information, "Getting customer events from");
        VerifyLogCalled(LogLevel.Information, $"Retrieved {expectedEvents.Count} customer events");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsExist()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        var query = new GetCustomerEventsQuery(fromDate, toDate);

        var emptyEvents = new List<CustomerEventData>();

        _repositoryMock.GetCustomerEventsAsync(fromDate, toDate)
                      .Returns(emptyEvents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        await _repositoryMock.Received(1).GetCustomerEventsAsync(fromDate, toDate);
        
        // Verify logging
        VerifyLogCalled(LogLevel.Information, "Retrieved 0 customer events");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        var query = new GetCustomerEventsQuery(fromDate, toDate);
        var expectedException = new Exception("Database connection failed");

        _repositoryMock.GetCustomerEventsAsync(fromDate, toDate)
                      .Returns(Task.FromException<IEnumerable<CustomerEventData>>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);
        
        await _repositoryMock.Received(1).GetCustomerEventsAsync(fromDate, toDate);
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
