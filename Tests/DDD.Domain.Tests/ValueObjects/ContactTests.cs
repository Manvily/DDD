using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class ContactTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreateContact_When_AllParametersAreValid()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var phone = _faker.Phone.PhoneNumber();

        // Act
        var contact = new Contact(email, phone);

        // Assert
        contact.Email.Should().Be(email);
        contact.Phone.Should().Be(phone);
    }

    [Fact]
    public void Constructor_Should_CreateContact_When_PhoneIsNull()
    {
        // Arrange
        var email = _faker.Internet.Email();
        string? phone = null;

        // Act
        var contact = new Contact(email, phone!);

        // Assert
        contact.Email.Should().Be(email);
        contact.Phone.Should().Be(phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_Should_ThrowArgumentException_When_EmailIsInvalid(string invalidEmail)
    {
        // Arrange
        var phone = _faker.Phone.PhoneNumber();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Contact(invalidEmail, phone));
        exception.ParamName.Should().Be("email");
        exception.Message.Should().Contain("Email cannot be empty");
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ContactsHaveSameValues()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var phone = _faker.Phone.PhoneNumber();

        var contact1 = new Contact(email, phone);
        var contact2 = new Contact(email, phone);

        // Act & Assert
        contact1.Should().Be(contact2);
        contact1.Equals(contact2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ContactsHaveDifferentEmails()
    {
        // Arrange
        var phone = _faker.Phone.PhoneNumber();
        var contact1 = new Contact("email1@example.com", phone);
        var contact2 = new Contact("email2@example.com", phone);

        // Act & Assert
        contact1.Should().NotBe(contact2);
        contact1.Equals(contact2).Should().BeFalse();
        (contact1 != contact2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ContactsHaveDifferentPhones()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var contact1 = new Contact(email, "123-456-7890");
        var contact2 = new Contact(email, "098-765-4321");

        // Act & Assert
        contact1.Should().NotBe(contact2);
        contact1.Equals(contact2).Should().BeFalse();
        (contact1 != contact2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_ContactsAreEqual()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var phone = _faker.Phone.PhoneNumber();

        var contact1 = new Contact(email, phone);
        var contact2 = new Contact(email, phone);

        // Act & Assert
        contact1.GetHashCode().Should().Be(contact2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_ContactsAreDifferent()
    {
        // Arrange
        var contact1 = new Contact("email1@example.com", "123-456-7890");
        var contact2 = new Contact("email2@example.com", "098-765-4321");

        // Act & Assert
        contact1.GetHashCode().Should().NotBe(contact2.GetHashCode());
    }

    [Fact]
    public void Constructor_Should_CreateContact_When_EmailHasValidFormat()
    {
        // Arrange
        var validEmails = new[]
        {
            "test@example.com",
            "user.name@domain.co.uk",
            "firstname+lastname@company.org",
            "email@123.123.123.123", // IP address
            "1234567890@example.com",
            "email@example-one.com"
        };

        foreach (var email in validEmails)
        {
            var phone = _faker.Phone.PhoneNumber();

            // Act & Assert
            var contact = new Contact(email, phone);
            contact.Email.Should().Be(email);
        }
    }
}
