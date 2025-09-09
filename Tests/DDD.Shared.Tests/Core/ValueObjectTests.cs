using AutoFixture;
using Bogus;
using FluentAssertions;
using Shared.Domain.Core;

namespace DDD.Shared.Tests.Core;

public class ValueObjectTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void EqualOperator_Should_ReturnTrue_When_BothObjectsAreNull()
    {
        // Arrange
        TestValueObject? left = null;
        TestValueObject? right = null;

        // Act
        var result = TestValueObject.EqualOperator(left, right);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_Should_ReturnFalse_When_OneObjectIsNull()
    {
        // Arrange
        var left = new TestValueObject("test");
        TestValueObject? right = null;

        // Act
        var result1 = TestValueObject.EqualOperator(left, right);
        var result2 = TestValueObject.EqualOperator(right, left);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Fact]
    public void EqualOperator_Should_ReturnTrue_When_ObjectsAreEqual()
    {
        // Arrange
        var value = _faker.Lorem.Word();
        var left = new TestValueObject(value);
        var right = new TestValueObject(value);

        // Act
        var result = TestValueObject.EqualOperator(left, right);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_Should_ReturnFalse_When_ObjectsAreNotEqual()
    {
        // Arrange
        var left = new TestValueObject("value1");
        var right = new TestValueObject("value2");

        // Act
        var result = TestValueObject.EqualOperator(left, right);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_Should_ReturnFalse_When_ObjectsAreEqual()
    {
        // Arrange
        var value = _faker.Lorem.Word();
        var left = new TestValueObject(value);
        var right = new TestValueObject(value);

        // Act
        var result = TestValueObject.NotEqualOperator(left, right);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_Should_ReturnTrue_When_ObjectsAreNotEqual()
    {
        // Arrange
        var left = new TestValueObject("value1");
        var right = new TestValueObject("value2");

        // Act
        var result = TestValueObject.NotEqualOperator(left, right);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var valueObject = new TestValueObject(_faker.Lorem.Word());

        // Act
        var result = valueObject.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ObjectIsOfDifferentType()
    {
        // Arrange
        var valueObject = new TestValueObject(_faker.Lorem.Word());
        var differentTypeObject = new object();

        // Act
        var result = valueObject.Equals(differentTypeObject);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ObjectsHaveSameEqualityComponents()
    {
        // Arrange
        var value = _faker.Lorem.Word();
        var valueObject1 = new TestValueObject(value);
        var valueObject2 = new TestValueObject(value);

        // Act
        var result = valueObject1.Equals(valueObject2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ObjectsHaveDifferentEqualityComponents()
    {
        // Arrange
        var valueObject1 = new TestValueObject("value1");
        var valueObject2 = new TestValueObject("value2");

        // Act
        var result = valueObject1.Equals(valueObject2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_ObjectsAreEqual()
    {
        // Arrange
        var value = _faker.Lorem.Word();
        var valueObject1 = new TestValueObject(value);
        var valueObject2 = new TestValueObject(value);

        // Act
        var hashCode1 = valueObject1.GetHashCode();
        var hashCode2 = valueObject2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_Should_ReturnDifferentValues_When_ObjectsAreDifferent()
    {
        // Arrange
        var valueObject1 = new TestValueObject("value1");
        var valueObject2 = new TestValueObject("value2");

        // Act
        var hashCode1 = valueObject1.GetHashCode();
        var hashCode2 = valueObject2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_Should_HandleNullComponents_When_ComponentIsNull()
    {
        // Arrange
        var valueObject = new TestValueObjectWithNullComponent();

        // Act
        var hashCode = valueObject.GetHashCode();

        // Assert
        hashCode.Should().Be(0); // XOR with 0 should return 0
    }

    [Fact]
    public void Equals_Should_HandleComplexEqualityComponents_When_MultipleComponents()
    {
        // Arrange
        var valueObject1 = new ComplexTestValueObject("value1", 42, DateTime.Today);
        var valueObject2 = new ComplexTestValueObject("value1", 42, DateTime.Today);
        var valueObject3 = new ComplexTestValueObject("value1", 43, DateTime.Today);

        // Act & Assert
        valueObject1.Equals(valueObject2).Should().BeTrue();
        valueObject1.Equals(valueObject3).Should().BeFalse();
        valueObject1.GetHashCode().Should().Be(valueObject2.GetHashCode());
        valueObject1.GetHashCode().Should().NotBe(valueObject3.GetHashCode());
    }

    // Test value object implementations
    private class TestValueObject : ValueObject
    {
        public string Value { get; }

        public TestValueObject(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        // Make the protected static methods accessible for testing
        public static new bool EqualOperator(ValueObject left, ValueObject right)
        {
            return ValueObject.EqualOperator(left, right);
        }

        public static new bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return ValueObject.NotEqualOperator(left, right);
        }
    }

    private class TestValueObjectWithNullComponent : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return null!;
        }
    }

    private class BaseValueObjectWithoutOverride : ValueObject
    {
        // Intentionally not overriding GetEqualityComponents to test the exception
    }

    private class ComplexTestValueObject : ValueObject
    {
        public string StringValue { get; }
        public int IntValue { get; }
        public DateTime DateValue { get; }

        public ComplexTestValueObject(string stringValue, int intValue, DateTime dateValue)
        {
            StringValue = stringValue;
            IntValue = intValue;
            DateValue = dateValue;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StringValue;
            yield return IntValue;
            yield return DateValue;
        }
    }
}
