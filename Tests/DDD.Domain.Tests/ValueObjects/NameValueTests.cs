using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class NameValueTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateNameValue_When_NameIsValid()
    {
        // Arrange
        var name = _faker.Commerce.ProductName();

        // Act
        var nameValue = new NameValue(name);

        // Assert
        nameValue.Name.Should().Be(name);
    }

    [Fact]
    public void Constructor_Should_CreateNameValue_When_NameIsEmpty()
    {
        // Arrange
        var name = string.Empty;

        // Act
        var nameValue = new NameValue(name);

        // Assert
        nameValue.Name.Should().Be(name);
    }

    [Fact]
    public void Constructor_Should_CreateNameValue_When_NameIsNull()
    {
        // Arrange
        string? name = null;

        // Act
        var nameValue = new NameValue(name!);

        // Assert
        nameValue.Name.Should().Be(name);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_NameValuesHaveSameName()
    {
        // Arrange
        var name = _faker.Commerce.ProductName();
        var nameValue1 = new NameValue(name);
        var nameValue2 = new NameValue(name);

        // Act & Assert
        nameValue1.Should().Be(nameValue2);
        nameValue1.Equals(nameValue2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_NameValuesHaveDifferentNames()
    {
        // Arrange
        var nameValue1 = new NameValue("Product A");
        var nameValue2 = new NameValue("Product B");

        // Act & Assert
        nameValue1.Should().NotBe(nameValue2);
        nameValue1.Equals(nameValue2).Should().BeFalse();
        (nameValue1 != nameValue2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_BothNamesAreNull()
    {
        // Arrange
        var nameValue1 = new NameValue(null!);
        var nameValue2 = new NameValue(null!);

        // Act & Assert
        nameValue1.Should().Be(nameValue2);
        nameValue1.Equals(nameValue2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_BothNamesAreEmpty()
    {
        // Arrange
        var nameValue1 = new NameValue(string.Empty);
        var nameValue2 = new NameValue(string.Empty);

        // Act & Assert
        nameValue1.Should().Be(nameValue2);
        nameValue1.Equals(nameValue2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_NameValuesAreEqual()
    {
        // Arrange
        var name = _faker.Commerce.ProductName();
        var nameValue1 = new NameValue(name);
        var nameValue2 = new NameValue(name);

        // Act & Assert
        nameValue1.GetHashCode().Should().Be(nameValue2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_NameValuesAreDifferent()
    {
        // Arrange
        var nameValue1 = new NameValue("Product A");
        var nameValue2 = new NameValue("Product B");

        // Act & Assert
        nameValue1.GetHashCode().Should().NotBe(nameValue2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_BothNamesAreNull()
    {
        // Arrange
        var nameValue1 = new NameValue(null!);
        var nameValue2 = new NameValue(null!);

        // Act & Assert
        nameValue1.GetHashCode().Should().Be(nameValue2.GetHashCode());
    }

    [Fact]
    public void GetEqualityComponents_Should_ReturnNameComponent_When_Called()
    {
        // Arrange
        var name = _faker.Commerce.ProductName();
        var nameValue = new NameValue(name);

        // Act
        var components = nameValue.GetType()
            .GetMethod("GetEqualityComponents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(nameValue, null) as IEnumerable<object>;

        // Assert
        components.Should().NotBeNull();
        components.Should().ContainSingle().Which.Should().Be(name);
    }

    [Fact]
    public void DefaultConstructor_Should_CreateNameValue_When_Called()
    {
        // Act
        var nameValue = new NameValue();

        // Assert
        nameValue.Should().NotBeNull();
        nameValue.Name.Should().BeNull(); // Default string value
    }
}
