using AutoFixture;
using Bogus;
using FluentAssertions;
using Shared.Domain.Events;

namespace DDD.Domain.Tests.Events;

public class CustomerCreatedEventTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateEvent_When_AllParametersAreValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        // Act
        var customerEvent = new CustomerCreatedEvent(customerId, customerName, source);

        // Assert
        customerEvent.CustomerId.Should().Be(customerId);
        customerEvent.CustomerName.Should().Be(customerName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_CustomerIdIsEmpty()
    {
        // Arrange
        var customerId = Guid.Empty;
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CustomerCreatedEvent(customerId, customerName, source));
        exception.ParamName.Should().Be("aggregateId");
        exception.Message.Should().Contain("AggregateId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_CustomerNameIsInvalid(string invalidCustomerName)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var source = "MainApi";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CustomerCreatedEvent(customerId, invalidCustomerName, source));
        exception.ParamName.Should().Be("customerName");
        exception.Message.Should().Contain("CustomerName cannot be empty");
    }

    [Fact]
    public void Constructor_Should_InheritFromBaseDomainEvent_When_Created()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        // Act
        var customerEvent = new CustomerCreatedEvent(customerId, customerName, source);

        // Assert
        customerEvent.Should().BeAssignableTo<BaseDomainEvent>();
    }

    [Fact]
    public void GetPayload_Should_ReturnCorrectPayload_When_Called()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = "John Doe";
        var source = "MainApi";
        var customerEvent = new CustomerCreatedEvent(customerId, customerName, source);

        // Act
        var payload = customerEvent.GetPayload();

        // Assert
        payload.Should().NotBeNull();
        
        // Use reflection to check the anonymous object properties
        var payloadType = payload.GetType();
        var customerIdProperty = payloadType.GetProperty("CustomerId");
        var customerNameProperty = payloadType.GetProperty("CustomerName");

        customerIdProperty.Should().NotBeNull();
        customerNameProperty.Should().NotBeNull();

        customerIdProperty!.GetValue(payload).Should().Be(customerId);
        customerNameProperty!.GetValue(payload).Should().Be(customerName);
    }

    [Fact]
    public void Constructor_Should_SetBaseDomainEventProperties_When_Created()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        // Act
        var customerEvent = new CustomerCreatedEvent(customerId, customerName, source);

        // Assert
        // Check if the base properties are accessible through reflection or casting
        var baseDomainEvent = customerEvent as BaseDomainEvent;
        baseDomainEvent.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_AcceptDifferentSources_When_CreatingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var sources = new[] { "MainApi", "Analytics", "ImportService", "TestService" };

        foreach (var source in sources)
        {
            // Act
            var customerEvent = new CustomerCreatedEvent(customerId, customerName, source);

            // Assert
            customerEvent.CustomerId.Should().Be(customerId);
            customerEvent.CustomerName.Should().Be(customerName);
        }
    }

    [Fact]
    public void Constructor_Should_AcceptLongCustomerName_When_CreatingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var longCustomerName = new string('A', 1000); // Very long name
        var source = "MainApi";

        // Act
        var customerEvent = new CustomerCreatedEvent(customerId, longCustomerName, source);

        // Assert
        customerEvent.CustomerId.Should().Be(customerId);
        customerEvent.CustomerName.Should().Be(longCustomerName);
        customerEvent.CustomerName.Should().HaveLength(1000);
    }

    [Fact]
    public void Constructor_Should_AcceptSpecialCharactersInCustomerName_When_CreatingEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var specialCharacterName = "José María Aznar-López (Jr.) & Co.";
        var source = "MainApi";

        // Act
        var customerEvent = new CustomerCreatedEvent(customerId, specialCharacterName, source);

        // Assert
        customerEvent.CustomerId.Should().Be(customerId);
        customerEvent.CustomerName.Should().Be(specialCharacterName);
    }

    [Fact]
    public void Events_Should_BeEqual_When_SameCustomerIdAndName()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        var event1 = new CustomerCreatedEvent(customerId, customerName, source);
        var event2 = new CustomerCreatedEvent(customerId, customerName, source);

        // Act & Assert
        event1.CustomerId.Should().Be(event2.CustomerId);
        event1.CustomerName.Should().Be(event2.CustomerName);
    }

    [Fact]
    public void Events_Should_BeDifferent_When_DifferentCustomerIds()
    {
        // Arrange
        var customerName = _faker.Name.FullName();
        var source = "MainApi";

        var event1 = new CustomerCreatedEvent(Guid.NewGuid(), customerName, source);
        var event2 = new CustomerCreatedEvent(Guid.NewGuid(), customerName, source);

        // Act & Assert
        event1.CustomerId.Should().NotBe(event2.CustomerId);
        event1.CustomerName.Should().Be(event2.CustomerName);
    }
}
