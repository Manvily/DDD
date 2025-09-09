using AutoFixture;
using Bogus;
using DDD.Domain.ValueObjects;
using FluentAssertions;

namespace DDD.Domain.Tests.ValueObjects;

public class PaymentStatusTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_Should_CreatePaymentStatus_When_IsPaidIsTrue()
    {
        // Arrange
        var isPaid = true;

        // Act
        var paymentStatus = new PaymentStatus(isPaid);

        // Assert
        paymentStatus.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void Constructor_Should_CreatePaymentStatus_When_IsPaidIsFalse()
    {
        // Arrange
        var isPaid = false;

        // Act
        var paymentStatus = new PaymentStatus(isPaid);

        // Assert
        paymentStatus.IsPaid.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_PaymentStatusesHaveSameValue()
    {
        // Arrange
        var isPaid = _faker.Random.Bool();
        var paymentStatus1 = new PaymentStatus(isPaid);
        var paymentStatus2 = new PaymentStatus(isPaid);

        // Act & Assert
        paymentStatus1.Should().Be(paymentStatus2);
        paymentStatus1.Equals(paymentStatus2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_PaymentStatusesHaveDifferentValues()
    {
        // Arrange
        var paymentStatus1 = new PaymentStatus(true);
        var paymentStatus2 = new PaymentStatus(false);

        // Act & Assert
        paymentStatus1.Should().NotBe(paymentStatus2);
        paymentStatus1.Equals(paymentStatus2).Should().BeFalse();
        (paymentStatus1 != paymentStatus2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_PaymentStatusesAreEqual()
    {
        // Arrange
        var isPaid = _faker.Random.Bool();
        var paymentStatus1 = new PaymentStatus(isPaid);
        var paymentStatus2 = new PaymentStatus(isPaid);

        // Act & Assert
        paymentStatus1.GetHashCode().Should().Be(paymentStatus2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_PaymentStatusesAreDifferent()
    {
        // Arrange
        var paymentStatus1 = new PaymentStatus(true);
        var paymentStatus2 = new PaymentStatus(false);

        // Act & Assert
        paymentStatus1.GetHashCode().Should().NotBe(paymentStatus2.GetHashCode());
    }

    [Fact]
    public void GetEqualityComponents_Should_ReturnIsPaidComponent_When_Called()
    {
        // Arrange
        var isPaid = _faker.Random.Bool();
        var paymentStatus = new PaymentStatus(isPaid);

        // Act
        var components = paymentStatus.GetType()
            .GetMethod("GetEqualityComponents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(paymentStatus, null) as IEnumerable<object>;

        // Assert
        components.Should().NotBeNull();
        components.Should().ContainSingle().Which.Should().Be(isPaid);
    }

    [Fact]
    public void DefaultConstructor_Should_CreatePaymentStatus_When_Called()
    {
        // Act
        var paymentStatus = new PaymentStatus();

        // Assert
        paymentStatus.Should().NotBeNull();
        paymentStatus.IsPaid.Should().BeFalse(); // Default bool value
    }
}
