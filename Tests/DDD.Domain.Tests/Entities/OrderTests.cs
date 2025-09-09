using AutoFixture;
using Bogus;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.Entities;

public class OrderTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateOrder_When_AllParametersAreValid()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var orderDate = _faker.Date.Recent();
        var products = CreateValidProducts();
        var paymentStatus = new PaymentStatus(true);

        // Act
        var order = new Order(customer, orderDate, products, paymentStatus);

        // Assert
        order.Customer.Should().Be(customer);
        order.OrderDate.Should().Be(orderDate);
        order.Products.Should().BeSameAs(products);
        order.Payment.Should().Be(paymentStatus);
    }

    [Fact]
    public void Constructor_Should_SetInitialState_When_CreatingOrder()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var orderDate = DateTime.Now;
        var products = CreateValidProducts();
        var paymentStatus = new PaymentStatus(false);

        // Act
        var order = new Order(customer, orderDate, products, paymentStatus);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().Be(default(Guid)); // Default value from Entity<Guid>
        order.Created.Should().Be(default(DateTime)); // Default value from Entity<Guid>
        order.Updated.Should().Be(default(DateTime)); // Default value from Entity<Guid>
    }

    [Fact]
    public void DefaultConstructor_Should_CreateOrder_When_Called()
    {
        // Act
        var order = new Order();

        // Assert
        order.Should().NotBeNull();
        order.Customer.Should().BeNull();
        order.OrderDate.Should().Be(default(DateTime));
        order.Products.Should().BeNull();
        order.Payment.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_BeSettable_When_UsingSetters()
    {
        // Arrange
        var order = new Order();
        var customer = CreateValidCustomer();
        var orderDate = DateTime.Now;
        var products = CreateValidProducts();
        var paymentStatus = new PaymentStatus(true);

        // Act
        order.Customer = customer;
        order.OrderDate = orderDate;
        order.Products = products;
        order.Payment = paymentStatus;

        // Assert
        order.Customer.Should().Be(customer);
        order.OrderDate.Should().Be(orderDate);
        order.Products.Should().BeSameAs(products);
        order.Payment.Should().Be(paymentStatus);
    }

    [Fact]
    public void Order_Should_InheritFromEntityOfGuid_When_Instantiated()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Should().BeAssignableTo<Shared.Domain.Core.Entity<Guid>>();
    }

    [Fact]
    public void Order_Should_HaveEntityProperties_When_Instantiated()
    {
        // Arrange
        var order = new Order();
        var id = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow.AddMinutes(1);

        // Act
        order.Id = id;
        order.Created = created;
        order.Updated = updated;

        // Assert
        order.Id.Should().Be(id);
        order.Created.Should().Be(created);
        order.Updated.Should().Be(updated);
    }

    [Fact]
    public void Constructor_Should_AcceptEmptyProductsList_When_CreatingOrder()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var orderDate = DateTime.Now;
        var products = new List<Product>();
        var paymentStatus = new PaymentStatus(false);

        // Act
        var order = new Order(customer, orderDate, products, paymentStatus);

        // Assert
        order.Products.Should().BeSameAs(products);
        order.Products.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_AcceptMultipleProducts_When_CreatingOrder()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var orderDate = DateTime.Now;
        var products = new List<Product>
        {
            CreateValidProduct("Product 1", 10.00m),
            CreateValidProduct("Product 2", 25.50m),
            CreateValidProduct("Product 3", 5.99m)
        };
        var paymentStatus = new PaymentStatus(true);

        // Act
        var order = new Order(customer, orderDate, products, paymentStatus);

        // Assert
        order.Products.Should().HaveCount(3);
        order.Products.Should().Contain(products);
    }

    [Fact]
    public void Payment_Should_AcceptDifferentPaymentStatuses_When_Set()
    {
        // Arrange
        var order = new Order();
        var paidStatus = new PaymentStatus(true);
        var unpaidStatus = new PaymentStatus(false);

        // Act & Assert - Paid status
        order.Payment = paidStatus;
        order.Payment.Should().Be(paidStatus);
        order.Payment.IsPaid.Should().BeTrue();

        // Act & Assert - Unpaid status
        order.Payment = unpaidStatus;
        order.Payment.Should().Be(unpaidStatus);
        order.Payment.IsPaid.Should().BeFalse();
    }

    private Customer CreateValidCustomer()
    {
        var name = new CustomerName(_faker.Name.FirstName(), _faker.Name.LastName());
        var contact = new Contact(_faker.Internet.Email(), _faker.Phone.PhoneNumber());
        var address = new Address(_faker.Address.City(), _faker.Address.ZipCode(), _faker.Address.StreetAddress(), _faker.Address.Country());
        var birthDate = _faker.Date.Past(50, DateTime.Now.AddYears(-18));

        return new Customer(name, contact, address, birthDate);
    }

    private List<Product> CreateValidProducts()
    {
        return new List<Product>
        {
            CreateValidProduct("Product 1", 19.99m),
            CreateValidProduct("Product 2", 29.99m)
        };
    }

    private Product CreateValidProduct(string productName, decimal price)
    {
        var name = new NameValue(productName);
        var productPrice = new Price(price);
        var category = new Category(new NameValue("Test Category"));

        return new Product(name, productPrice, category);
    }
}
