using AutoFixture;
using Bogus;
using DDD.Domain.Entities;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.Entities;

public class CategoryTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateCategory_When_NameIsValid()
    {
        // Arrange
        var name = new NameValue(_faker.Commerce.Categories(1).First());

        // Act
        var category = new Category(name);

        // Assert
        category.Name.Should().Be(name);
        category.Products.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetInitialState_When_CreatingCategory()
    {
        // Arrange
        var name = new NameValue("Electronics");

        // Act
        var category = new Category(name);

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().Be(default(Guid)); // Default value from Entity<Guid>
        category.Created.Should().Be(default(DateTime)); // Default value from Entity<Guid>
        category.Updated.Should().Be(default(DateTime)); // Default value from Entity<Guid>
    }

    [Fact]
    public void DefaultConstructor_Should_CreateCategory_When_Called()
    {
        // Act
        var category = new Category();

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().BeNull();
        category.Products.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_BeSettable_When_UsingSetters()
    {
        // Arrange
        var category = new Category();
        var name = new NameValue("Updated Category");
        var products = new List<Product>();

        // Act
        category.Name = name;
        category.Products = products;

        // Assert
        category.Name.Should().Be(name);
        category.Products.Should().BeSameAs(products);
    }

    [Fact]
    public void Category_Should_InheritFromEntityOfGuid_When_Instantiated()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        category.Should().BeAssignableTo<Shared.Domain.Core.Entity<Guid>>();
    }

    [Fact]
    public void Category_Should_HaveEntityProperties_When_Instantiated()
    {
        // Arrange
        var category = new Category();
        var id = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow.AddMinutes(1);

        // Act
        category.Id = id;
        category.Created = created;
        category.Updated = updated;

        // Assert
        category.Id.Should().Be(id);
        category.Created.Should().Be(created);
        category.Updated.Should().Be(updated);
    }

    [Fact]
    public void Constructor_Should_AcceptEmptyNameValue_When_CreatingCategory()
    {
        // Arrange
        var name = new NameValue(string.Empty);

        // Act
        var category = new Category(name);

        // Assert
        category.Name.Should().Be(name);
        category.Name.Name.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_AcceptNullNameValue_When_CreatingCategory()
    {
        // Arrange
        var name = new NameValue(null!);

        // Act
        var category = new Category(name);

        // Assert
        category.Name.Should().Be(name);
        category.Name.Name.Should().BeNull();
    }

    [Fact]
    public void Category_Should_AllowProductsToBeSet_When_UsingProperty()
    {
        // Arrange
        var category = new Category();
        var products = new List<Product>
        {
            CreateValidProduct("Product 1"),
            CreateValidProduct("Product 2"),
            CreateValidProduct("Product 3")
        };

        // Act
        category.Products = products;

        // Assert
        category.Products.Should().BeSameAs(products);
        category.Products.Should().HaveCount(3);
    }

    [Fact]
    public void Category_Should_AllowEmptyProductsList_When_Set()
    {
        // Arrange
        var category = new Category();
        var products = new List<Product>();

        // Act
        category.Products = products;

        // Assert
        category.Products.Should().BeSameAs(products);
        category.Products.Should().BeEmpty();
    }

    [Fact]
    public void Name_Should_AllowDifferentCategoryNames_When_Set()
    {
        // Arrange
        var category = new Category();
        var electronicsName = new NameValue("Electronics");
        var clothingName = new NameValue("Clothing");
        var booksName = new NameValue("Books & Media");

        // Act & Assert
        category.Name = electronicsName;
        category.Name.Should().Be(electronicsName);
        category.Name.Name.Should().Be("Electronics");

        category.Name = clothingName;
        category.Name.Should().Be(clothingName);
        category.Name.Name.Should().Be("Clothing");

        category.Name = booksName;
        category.Name.Should().Be(booksName);
        category.Name.Name.Should().Be("Books & Media");
    }

    [Fact]
    public void Constructor_Should_CreateDifferentCategories_When_CalledMultipleTimes()
    {
        // Arrange
        var name1 = new NameValue("Category 1");
        var name2 = new NameValue("Category 2");

        // Act
        var category1 = new Category(name1);
        var category2 = new Category(name2);

        // Assert
        category1.Should().NotBeSameAs(category2);
        category1.Name.Should().NotBe(category2.Name);
        category1.Id.Should().Be(Guid.Empty); // Both have default Guid until explicitly set
        category2.Id.Should().Be(Guid.Empty);
    }

    private Product CreateValidProduct(string productName)
    {
        var name = new NameValue(productName);
        var price = new Price(_faker.Random.Decimal(1, 1000));
        var category = new Category(new NameValue("Test Category"));

        return new Product(name, price, category);
    }
}
