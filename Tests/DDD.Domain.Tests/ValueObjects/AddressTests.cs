using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class AddressTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateAddress_When_AllParametersAreValid()
    {
        // Arrange
        var city = _faker.Address.City();
        var zipCode = _faker.Address.ZipCode();
        var street = _faker.Address.StreetAddress();
        var country = _faker.Address.Country();

        // Act
        var address = new Address(city, zipCode, street, country);

        // Assert
        address.City.Should().Be(city);
        address.ZipCode.Should().Be(zipCode);
        address.Street.Should().Be(street);
        address.Country.Should().Be(country);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_CityIsInvalid(string invalidCity)
    {
        // Arrange
        var zipCode = _faker.Address.ZipCode();
        var street = _faker.Address.StreetAddress();
        var country = _faker.Address.Country();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(invalidCity, zipCode, street, country));
        exception.ParamName.Should().Be("city");
        exception.Message.Should().Contain("City cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_ZipCodeIsInvalid(string invalidZipCode)
    {
        // Arrange
        var city = _faker.Address.City();
        var street = _faker.Address.StreetAddress();
        var country = _faker.Address.Country();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(city, invalidZipCode, street, country));
        exception.ParamName.Should().Be("zipCode");
        exception.Message.Should().Contain("Zip code cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_StreetIsInvalid(string invalidStreet)
    {
        // Arrange
        var city = _faker.Address.City();
        var zipCode = _faker.Address.ZipCode();
        var country = _faker.Address.Country();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(city, zipCode, invalidStreet, country));
        exception.ParamName.Should().Be("street");
        exception.Message.Should().Contain("Street cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_CountryIsInvalid(string invalidCountry)
    {
        // Arrange
        var city = _faker.Address.City();
        var zipCode = _faker.Address.ZipCode();
        var street = _faker.Address.StreetAddress();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Address(city, zipCode, street, invalidCountry));
        exception.ParamName.Should().Be("country");
        exception.Message.Should().Contain("Country cannot be empty");
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_AddressesHaveSameValues()
    {
        // Arrange
        var city = _faker.Address.City();
        var zipCode = _faker.Address.ZipCode();
        var street = _faker.Address.StreetAddress();
        var country = _faker.Address.Country();

        var address1 = new Address(city, zipCode, street, country);
        var address2 = new Address(city, zipCode, street, country);

        // Act & Assert
        address1.Should().Be(address2);
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_AddressesHaveDifferentValues()
    {
        // Arrange
        var address1 = _fixture.Create<Address>();
        var address2 = _fixture.Create<Address>();

        // Act & Assert
        address1.Should().NotBe(address2);
        address1.Equals(address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_AddressesAreEqual()
    {
        // Arrange
        var city = _faker.Address.City();
        var zipCode = _faker.Address.ZipCode();
        var street = _faker.Address.StreetAddress();
        var country = _faker.Address.Country();

        var address1 = new Address(city, zipCode, street, country);
        var address2 = new Address(city, zipCode, street, country);

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_AddressesAreDifferent()
    {
        // Arrange
        var address1 = new Address("City1", "12345", "Street1", "Country1");
        var address2 = new Address("City2", "67890", "Street2", "Country2");

        // Act & Assert
        address1.GetHashCode().Should().NotBe(address2.GetHashCode());
    }
}
