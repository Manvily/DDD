using AutoFixture;
using Bogus;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.Entities;

public class ProductTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateProduct_When_AllParametersAreValid()
    {
        // Arrange
        var name = new NameValue(_faker.Commerce.ProductName());
        var price = new Price(_faker.Random.Decimal(1, 1000));
        var category = CreateValidCategory();

        // Act
        var product = new Product(name, price, category);

        // Assert
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.Category.Should().Be(category);
        product.Orders.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetInitialState_When_CreatingProduct()
    {
        // Arrange
        var name = new NameValue("Test Product");
        var price = new Price(99.99m);
        var category = CreateValidCategory();

        // Act
        var product = new Product(name, price, category);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().Be(default(Guid)); // Default value from Entity<Guid>
        product.Created.Should().Be(default(DateTime)); // Default value from Entity<Guid>
        product.Updated.Should().Be(default(DateTime)); // Default value from Entity<Guid>
    }

    [Fact]
    public void DefaultConstructor_Should_CreateProduct_When_Called()
    {
        // Act
        var product = new Product();

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().BeNull();
        product.Price.Should().BeNull();
        product.Category.Should().BeNull();
        product.Orders.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_BeSettable_When_UsingSetters()
    {
        // Arrange
        var product = new Product();
        var name = new NameValue("Updated Product");
        var price = new Price(149.99m);
        var category = CreateValidCategory();
        var orders = new List<Order>();

        // Act
        product.Name = name;
        product.Price = price;
        product.Category = category;
        product.Orders = orders;

        // Assert
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.Category.Should().Be(category);
        product.Orders.Should().BeSameAs(orders);
    }

    [Fact]
    public void Product_Should_InheritFromEntityOfGuid_When_Instantiated()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Should().BeAssignableTo<Shared.Domain.Core.Entity<Guid>>();
    }

    [Fact]
    public void Product_Should_HaveEntityProperties_When_Instantiated()
    {
        // Arrange
        var product = new Product();
        var id = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow.AddMinutes(1);

        // Act
        product.Id = id;
        product.Created = created;
        product.Updated = updated;

        // Assert
        product.Id.Should().Be(id);
        product.Created.Should().Be(created);
        product.Updated.Should().Be(updated);
    }

    [Fact]
    public void Constructor_Should_AcceptZeroPrice_When_CreatingProduct()
    {
        // Arrange
        var name = new NameValue("Free Product");
        var price = new Price(0m);
        var category = CreateValidCategory();

        // Act
        var product = new Product(name, price, category);

        // Assert
        product.Price.Should().Be(price);
        product.Price.Value.Should().Be(0m);
    }

    [Fact]
    public void Constructor_Should_AcceptHighPrice_When_CreatingProduct()
    {
        // Arrange
        var name = new NameValue("Expensive Product");
        var price = new Price(9999.99m);
        var category = CreateValidCategory();

        // Act
        var product = new Product(name, price, category);

        // Assert
        product.Price.Should().Be(price);
        product.Price.Value.Should().Be(9999.99m);
    }

    [Fact]
    public void Product_Should_AllowOrdersToBeSet_When_UsingProperty()
    {
        // Arrange
        var product = new Product();
        var orders = new List<Order>
        {
            new Order(),
            new Order()
        };

        // Act
        product.Orders = orders;

        // Assert
        product.Orders.Should().BeSameAs(orders);
        product.Orders.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_Should_AcceptDifferentCategories_When_CreatingProduct()
    {
        // Arrange
        var name = new NameValue("Test Product");
        var price = new Price(50.00m);
        var electronicsCategory = new Category(new NameValue("Electronics"));
        var clothingCategory = new Category(new NameValue("Clothing"));

        // Act
        var electronicsProduct = new Product(name, price, electronicsCategory);
        var clothingProduct = new Product(name, price, clothingCategory);

        // Assert
        electronicsProduct.Category.Should().Be(electronicsCategory);
        clothingProduct.Category.Should().Be(clothingCategory);
        electronicsProduct.Category.Should().NotBe(clothingProduct.Category);
    }

    [Fact]
    public void Name_Should_AllowDifferentProductNames_When_Set()
    {
        // Arrange
        var product = new Product();
        var shortName = new NameValue("A");
        var longName = new NameValue("A Very Long Product Name With Many Words");
        var specialCharName = new NameValue("Product-123 & Co. (Special Edition)");

        // Act & Assert
        product.Name = shortName;
        product.Name.Should().Be(shortName);

        product.Name = longName;
        product.Name.Should().Be(longName);

        product.Name = specialCharName;
        product.Name.Should().Be(specialCharName);
    }

    private Category CreateValidCategory()
    {
        var categoryName = new NameValue(_faker.Commerce.Categories(1).First());
        return new Category(categoryName);
    }
}
