using AutoFixture;
using Bogus;
using FluentAssertions;
using Shared.Domain.Core;
using Shared.Domain.Events;

namespace DDD.Shared.Tests.Events;

public class BaseDomainEventTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateEvent_When_AllParametersAreValid()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var version = "2.0";

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType, version);

        // Assert
        domainEvent.Source.Should().Be(source);
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.AggregateType.Should().Be(aggregateType);
        domainEvent.Version.Should().Be(version);
        domainEvent.EventType.Should().Be("TestDomainEvent");
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_Should_UseDefaultVersion_When_VersionIsNotProvided()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType);

        // Assert
        domainEvent.Version.Should().Be("1.0");
    }

    [Fact]
    public void Constructor_Should_GenerateUniqueEventId_When_CreatingMultipleEvents()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";

        // Act
        var event1 = new TestDomainEvent(source, aggregateId, aggregateType);
        var event2 = new TestDomainEvent(source, aggregateId, aggregateType);

        // Assert
        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void Constructor_Should_SetOccurredOnToCurrentTime_When_CreatingEvent()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredOn.Should().BeAfter(beforeCreation.AddMilliseconds(-1));
        domainEvent.OccurredOn.Should().BeBefore(afterCreation.AddMilliseconds(1));
    }

    [Fact]
    public void Constructor_Should_SetEventTypeToClassName_When_CreatingEvent()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType);

        // Assert
        domainEvent.EventType.Should().Be(typeof(TestDomainEvent).Name);
        domainEvent.EventType.Should().Be("TestDomainEvent");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_SourceIsInvalid(string invalidSource)
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new TestDomainEvent(invalidSource, aggregateId, aggregateType));
        exception.ParamName.Should().Be("source");
        exception.Message.Should().Contain("Source cannot be empty");
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_AggregateIdIsEmpty()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.Empty;
        var aggregateType = "TestAggregate";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new TestDomainEvent(source, aggregateId, aggregateType));
        exception.ParamName.Should().Be("aggregateId");
        exception.Message.Should().Contain("AggregateId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_AggregateTypeIsInvalid(string invalidAggregateType)
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new TestDomainEvent(source, aggregateId, invalidAggregateType));
        exception.ParamName.Should().Be("aggregateType");
        exception.Message.Should().Contain("AggregateType cannot be empty");
    }

    [Fact]
    public void BaseDomainEvent_Should_ImplementIDomainEvent_When_Created()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType);

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void GetPayload_Should_BeAbstract_When_CalledOnBaseDomainEvent()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType);

        // Act
        var payload = domainEvent.GetPayload();

        // Assert
        payload.Should().NotBeNull();
        payload.Should().Be("TestPayload");
    }

    [Fact]
    public void Constructor_Should_AcceptDifferentVersionFormats_When_CreatingEvent()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var versions = new[] { "1.0", "2.1", "1.0.0", "2.0.1-beta", "3.0.0-rc.1" };

        foreach (var version in versions)
        {
            // Act
            var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType, version);

            // Assert
            domainEvent.Version.Should().Be(version);
        }
    }

    [Fact]
    public void Properties_Should_BeReadOnly_When_SetInConstructor()
    {
        // Arrange
        var source = "TestSource";
        var aggregateId = Guid.NewGuid();
        var aggregateType = "TestAggregate";
        var version = "2.0";

        // Act
        var domainEvent = new TestDomainEvent(source, aggregateId, aggregateType, version);

        // Assert
        // Verify that properties don't have public setters by checking the property info
        var type = typeof(BaseDomainEvent);
        
        type.GetProperty(nameof(BaseDomainEvent.EventId))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.AggregateId))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.OccurredOn))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.AggregateType))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.EventType))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.Source))!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(BaseDomainEvent.Version))!.CanWrite.Should().BeFalse();
    }

    // Test implementation of BaseDomainEvent
    private class TestDomainEvent : BaseDomainEvent
    {
        public TestDomainEvent(string source, Guid aggregateId, string aggregateType, string version = "1.0")
            : base(source, aggregateId, aggregateType, version)
        {
        }

        public override object GetPayload()
        {
            return "TestPayload";
        }
    }
}
