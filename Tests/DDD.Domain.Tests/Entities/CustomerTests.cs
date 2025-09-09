using AutoFixture;
using Bogus;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.Entities;

public class CustomerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateCustomer_When_AllParametersAreValid()
    {
        // Arrange
        var name = new CustomerName(_faker.Name.FirstName(), _faker.Name.LastName());
        var contact = new Contact(_faker.Internet.Email(), _faker.Phone.PhoneNumber());
        var address = new Address(_faker.Address.City(), _faker.Address.ZipCode(), _faker.Address.StreetAddress(), _faker.Address.Country());
        var birthDate = _faker.Date.Past(50, DateTime.Now.AddYears(-18));

        // Act
        var customer = new Customer(name, contact, address, birthDate);

        // Assert
        customer.Name.Should().Be(name);
        customer.Contact.Should().Be(contact);
        customer.Address.Should().Be(address);
        customer.BirthDate.Should().Be(birthDate);
        customer.Orders.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetInitialState_When_CreatingCustomer()
    {
        // Arrange
        var name = new CustomerName("John", "Doe");
        var contact = new Contact("john.doe@example.com", "123-456-7890");
        var address = new Address("New York", "10001", "123 Main St", "USA");
        var birthDate = new DateTime(1990, 1, 1);

        // Act
        var customer = new Customer(name, contact, address, birthDate);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().Be(default(Guid)); // Default value from Entity<Guid>
        customer.Created.Should().Be(default(DateTime)); // Default value from Entity<Guid>
        customer.Updated.Should().Be(default(DateTime)); // Default value from Entity<Guid>
    }

    [Fact]
    public void DefaultConstructor_Should_CreateCustomer_When_Called()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.Should().NotBeNull();
        customer.Name.Should().BeNull();
        customer.Contact.Should().BeNull();
        customer.Address.Should().BeNull();
        customer.BirthDate.Should().Be(default(DateTime));
        customer.Orders.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_BeSettable_When_UsingSetters()
    {
        // Arrange
        var customer = new Customer();
        var name = new CustomerName("Jane", "Smith");
        var contact = new Contact("jane.smith@example.com", "987-654-3210");
        var address = new Address("Los Angeles", "90210", "456 Oak Ave", "USA");
        var birthDate = new DateTime(1985, 5, 15);
        var orders = new List<Order>();

        // Act
        customer.Name = name;
        customer.Contact = contact;
        customer.Address = address;
        customer.BirthDate = birthDate;
        customer.Orders = orders;

        // Assert
        customer.Name.Should().Be(name);
        customer.Contact.Should().Be(contact);
        customer.Address.Should().Be(address);
        customer.BirthDate.Should().Be(birthDate);
        customer.Orders.Should().BeSameAs(orders);
    }

    [Fact]
    public void Customer_Should_InheritFromEntityOfGuid_When_Instantiated()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.Should().BeAssignableTo<Shared.Domain.Core.Entity<Guid>>();
    }

    [Fact]
    public void Customer_Should_HaveEntityProperties_When_Instantiated()
    {
        // Arrange
        var customer = new Customer();
        var id = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow.AddMinutes(1);

        // Act
        customer.Id = id;
        customer.Created = created;
        customer.Updated = updated;

        // Assert
        customer.Id.Should().Be(id);
        customer.Created.Should().Be(created);
        customer.Updated.Should().Be(updated);
    }

    [Fact]
    public void Constructor_Should_AcceptNullOrders_When_CreatingCustomer()
    {
        // Arrange
        var name = new CustomerName("Test", "User");
        var contact = new Contact("test@example.com", "123-456-7890");
        var address = new Address("City", "12345", "Street", "Country");
        var birthDate = DateTime.Now.AddYears(-25);

        // Act
        var customer = new Customer(name, contact, address, birthDate);

        // Assert
        customer.Orders.Should().BeNull();
    }

    [Fact]
    public void Customer_Should_AllowOrdersToBeSet_When_UsingProperty()
    {
        // Arrange
        var customer = new Customer();
        var orders = new List<Order>
        {
            new Order(),
            new Order()
        };

        // Act
        customer.Orders = orders;

        // Assert
        customer.Orders.Should().BeSameAs(orders);
        customer.Orders.Should().HaveCount(2);
    }
}
