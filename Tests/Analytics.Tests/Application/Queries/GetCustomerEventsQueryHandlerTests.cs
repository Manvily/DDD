using Analytics.Application.Abstractions;
using Analytics.Application.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Analytics.Tests.Application.Queries;

public class GetCustomerEventsQueryHandlerTests
{
    private readonly Mock<IAnalyticsEventQueryRepository> _repositoryMock;
    private readonly Mock<ILogger<GetCustomerEventsQueryHandler>> _loggerMock;
    private readonly GetCustomerEventsQueryHandler _handler;

    public GetCustomerEventsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAnalyticsEventQueryRepository>();
        _loggerMock = new Mock<ILogger<GetCustomerEventsQueryHandler>>();
        _handler = new GetCustomerEventsQueryHandler(_repositoryMock.Object, _loggerMock.Object);
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

        _repositoryMock.Setup(x => x.GetCustomerEventsAsync(fromDate, toDate))
                      .ReturnsAsync(expectedEvents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedEvents);
        _repositoryMock.Verify(x => x.GetCustomerEventsAsync(fromDate, toDate), Times.Once);
        
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

        _repositoryMock.Setup(x => x.GetCustomerEventsAsync(fromDate, toDate))
                      .ReturnsAsync(emptyEvents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetCustomerEventsAsync(fromDate, toDate), Times.Once);
        
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

        _repositoryMock.Setup(x => x.GetCustomerEventsAsync(fromDate, toDate))
                      .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        exception.Should().BeEquivalentTo(expectedException);
        
        _repositoryMock.Verify(x => x.GetCustomerEventsAsync(fromDate, toDate), Times.Once);
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
