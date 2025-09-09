using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class PriceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreatePrice_When_ValueIsValid()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);

        // Act
        var price = new Price(value);

        // Assert
        price.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_Should_CreatePrice_When_ValueIsZero()
    {
        // Arrange
        var value = 0m;

        // Act
        var price = new Price(value);

        // Assert
        price.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNegative()
    {
        // Arrange
        var negativeValue = -1m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Price(negativeValue));
        exception.Message.Should().Contain("Price cannot be negative");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-1000.50)]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNegative_WithVariousValues(decimal negativeValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Price(negativeValue));
        exception.Message.Should().Contain("Price cannot be negative");
    }

    [Fact]
    public void ImplicitOperator_Should_ConvertPriceToDecimal_When_Called()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);
        var price = new Price(value);

        // Act
        decimal result = price;

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void ImplicitOperator_Should_ConvertDecimalToPrice_When_Called()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);

        // Act
        Price price = value;

        // Assert
        price.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitOperator_Should_ThrowArgumentException_When_ConvertingNegativeDecimalToPrice()
    {
        // Arrange
        var negativeValue = -1m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => { Price price = negativeValue; });
        exception.Message.Should().Contain("Price cannot be negative");
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_PricesHaveSameValue()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);
        var price1 = new Price(value);
        var price2 = new Price(value);

        // Act & Assert
        price1.Should().Be(price2);
        price1.Equals(price2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_PricesHaveDifferentValues()
    {
        // Arrange
        var price1 = new Price(100m);
        var price2 = new Price(200m);

        // Act & Assert
        price1.Should().NotBe(price2);
        price1.Equals(price2).Should().BeFalse();
        (price1 != price2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_PricesAreEqual()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);
        var price1 = new Price(value);
        var price2 = new Price(value);

        // Act & Assert
        price1.GetHashCode().Should().Be(price2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_PricesAreDifferent()
    {
        // Arrange
        var price1 = new Price(100m);
        var price2 = new Price(200m);

        // Act & Assert
        price1.GetHashCode().Should().NotBe(price2.GetHashCode());
    }

    [Fact]
    public void GetEqualityComponents_Should_ReturnValueComponent_When_Called()
    {
        // Arrange
        var value = _faker.Random.Decimal(0, 10000);
        var price = new Price(value);

        // Act
        var components = price.GetType()
            .GetMethod("GetEqualityComponents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(price, null) as IEnumerable<object>;

        // Assert
        components.Should().NotBeNull();
        components.Should().ContainSingle().Which.Should().Be(value);
    }
}
