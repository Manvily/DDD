using AutoFixture;
using Bogus;
using FluentAssertions;
using Shared.Domain.Events;

namespace DDD.Domain.Tests.Events;

public class OrderCreatedEventTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateEvent_When_AllParametersAreValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        // Act
        var orderEvent = new OrderCreatedEvent(customerId, source, orderId);

        // Assert
        orderEvent.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_CustomerIdIsEmpty()
    {
        // Arrange
        var customerId = Guid.Empty;
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new OrderCreatedEvent(customerId, source, orderId));
        exception.ParamName.Should().Be("customerId");
        exception.Message.Should().Contain("CustomerId cannot be empty");
    }

    [Fact]
    public void Constructor_Should_InheritFromBaseDomainEvent_When_Created()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        // Act
        var orderEvent = new OrderCreatedEvent(customerId, source, orderId);

        // Assert
        orderEvent.Should().BeAssignableTo<BaseDomainEvent>();
    }

    [Fact]
    public void GetPayload_Should_ReturnCorrectPayload_When_Called()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";
        var orderEvent = new OrderCreatedEvent(customerId, source, orderId);

        // Act
        var payload = orderEvent.GetPayload();

        // Assert
        payload.Should().NotBeNull();
        
        // Use reflection to check the anonymous object properties
        var payloadType = payload.GetType();
        var customerIdProperty = payloadType.GetProperty("CustomerId");

        customerIdProperty.Should().NotBeNull();
        customerIdProperty!.GetValue(payload).Should().Be(customerId);
    }

    [Fact]
    public void Constructor_Should_SetBaseDomainEventProperties_When_Created()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        // Act
        var orderEvent = new OrderCreatedEvent(customerId, source, orderId);

        // Assert
        var baseDomainEvent = orderEvent as BaseDomainEvent;
        baseDomainEvent.Should().NotBeNull();
        baseDomainEvent!.AggregateId.Should().Be(orderId);
        baseDomainEvent.Source.Should().Be(source);
        baseDomainEvent.AggregateType.Should().Be("Order");
        baseDomainEvent.Version.Should().Be("1.0");
        baseDomainEvent.EventType.Should().Be("OrderCreatedEvent");
        baseDomainEvent.EventId.Should().NotBeEmpty();
        baseDomainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_Should_AcceptDifferentSources_When_CreatingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var sources = new[] { "MainApi", "Analytics", "OrderService", "TestService" };

        foreach (var source in sources)
        {
            // Act
            var orderEvent = new OrderCreatedEvent(customerId, source, orderId);

            // Assert
            orderEvent.CustomerId.Should().Be(customerId);
            var baseDomainEvent = orderEvent as BaseDomainEvent;
            baseDomainEvent!.Source.Should().Be(source);
        }
    }

    [Fact]
    public void Events_Should_BeEqual_When_SameCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var source = "MainApi";

        var event1 = new OrderCreatedEvent(customerId, source, orderId1);
        var event2 = new OrderCreatedEvent(customerId, source, orderId2);

        // Act & Assert
        event1.CustomerId.Should().Be(event2.CustomerId);
        // Note: The events themselves are different instances with different order IDs
        (event1 as BaseDomainEvent)!.AggregateId.Should().NotBe((event2 as BaseDomainEvent)!.AggregateId);
    }

    [Fact]
    public void Events_Should_BeDifferent_When_DifferentCustomerIds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        var event1 = new OrderCreatedEvent(Guid.NewGuid(), source, orderId);
        var event2 = new OrderCreatedEvent(Guid.NewGuid(), source, orderId);

        // Act & Assert
        event1.CustomerId.Should().NotBe(event2.CustomerId);
        (event1 as BaseDomainEvent)!.AggregateId.Should().Be((event2 as BaseDomainEvent)!.AggregateId);
    }

    [Fact]
    public void Constructor_Should_GenerateUniqueEventIds_When_CreatingMultipleEvents()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";

        // Act
        var event1 = new OrderCreatedEvent(customerId, source, orderId);
        var event2 = new OrderCreatedEvent(customerId, source, orderId);

        // Assert
        var baseEvent1 = event1 as BaseDomainEvent;
        var baseEvent2 = event2 as BaseDomainEvent;
        
        baseEvent1!.EventId.Should().NotBe(baseEvent2!.EventId);
    }

    [Fact]
    public void Constructor_Should_SetOccurredOnToCurrentTime_When_CreatingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var source = "MainApi";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var orderEvent = new OrderCreatedEvent(customerId, source, orderId);
        var afterCreation = DateTime.UtcNow;

        // Assert
        var baseDomainEvent = orderEvent as BaseDomainEvent;
        baseDomainEvent!.OccurredOn.Should().BeAfter(beforeCreation.AddMilliseconds(-1));
        baseDomainEvent.OccurredOn.Should().BeBefore(afterCreation.AddMilliseconds(1));
    }
}
