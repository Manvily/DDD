using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class CustomerNameTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateCustomerName_When_AllParametersAreValid()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();

        // Act
        var customerName = new CustomerName(firstName, lastName);

        // Assert
        customerName.First.Should().Be(firstName);
        customerName.Last.Should().Be(lastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_FirstNameIsInvalid(string invalidFirstName)
    {
        // Arrange
        var lastName = _faker.Name.LastName();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CustomerName(invalidFirstName, lastName));
        exception.ParamName.Should().Be("first");
        exception.Message.Should().Contain("First name cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_LastNameIsInvalid(string invalidLastName)
    {
        // Arrange
        var firstName = _faker.Name.FirstName();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CustomerName(firstName, invalidLastName));
        exception.ParamName.Should().Be("last");
        exception.Message.Should().Contain("Last name cannot be empty");
    }

    [Fact]
    public void ToString_Should_ReturnFormattedName_When_Called()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var customerName = new CustomerName(firstName, lastName);

        // Act
        var result = customerName.ToString();

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_CustomerNamesHaveSameValues()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();

        var customerName1 = new CustomerName(firstName, lastName);
        var customerName2 = new CustomerName(firstName, lastName);

        // Act & Assert
        customerName1.Should().Be(customerName2);
        customerName1.Equals(customerName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_CustomerNamesHaveDifferentValues()
    {
        // Arrange
        var customerName1 = new CustomerName("John", "Doe");
        var customerName2 = new CustomerName("Jane", "Smith");

        // Act & Assert
        customerName1.Should().NotBe(customerName2);
        customerName1.Equals(customerName2).Should().BeFalse();
        (customerName1 != customerName2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_CustomerNamesAreEqual()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();

        var customerName1 = new CustomerName(firstName, lastName);
        var customerName2 = new CustomerName(firstName, lastName);

        // Act & Assert
        customerName1.GetHashCode().Should().Be(customerName2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_CustomerNamesAreDifferent()
    {
        // Arrange
        var customerName1 = new CustomerName("John", "Doe");
        var customerName2 = new CustomerName("Jane", "Smith");

        // Act & Assert
        customerName1.GetHashCode().Should().NotBe(customerName2.GetHashCode());
    }
}
