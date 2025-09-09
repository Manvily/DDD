using AutoFixture;
using Bogus;
using FluentAssertions;
using Shared.Domain.Core;

namespace DDD.Shared.Tests.Core;

public class EntityTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Faker _faker = new();

    [Fact]
    public void Entity_Should_HaveDefaultValues_When_Instantiated()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(default(Guid));
        entity.Created.Should().Be(default(DateTime));
        entity.Updated.Should().Be(default(DateTime));
    }

    [Fact]
    public void Entity_Should_AllowSettingId_When_PropertyIsSet()
    {
        // Arrange
        var entity = new TestEntity();
        var id = Guid.NewGuid();

        // Act
        entity.Id = id;

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Entity_Should_AllowSettingCreated_When_PropertyIsSet()
    {
        // Arrange
        var entity = new TestEntity();
        var created = DateTime.UtcNow;

        // Act
        entity.Created = created;

        // Assert
        entity.Created.Should().Be(created);
    }

    [Fact]
    public void Entity_Should_AllowSettingUpdated_When_PropertyIsSet()
    {
        // Arrange
        var entity = new TestEntity();
        var updated = DateTime.UtcNow;

        // Act
        entity.Updated = updated;

        // Assert
        entity.Updated.Should().Be(updated);
    }

    [Fact]
    public void Entity_Should_SupportDifferentIdTypes_When_UsingGenericType()
    {
        // Arrange & Act
        var guidEntity = new TestEntityWithGuid();
        var intEntity = new TestEntityWithInt();
        var stringEntity = new TestEntityWithString();

        // Assert
        guidEntity.Id.Should().Be(default(Guid));
        intEntity.Id.Should().Be(default(int));
        stringEntity.Id.Should().Be(default(string));
    }

    [Fact]
    public void Entity_Should_AllowSettingDifferentIdTypes_When_PropertiesAreSet()
    {
        // Arrange
        var guidEntity = new TestEntityWithGuid();
        var intEntity = new TestEntityWithInt();
        var stringEntity = new TestEntityWithString();

        var guidId = Guid.NewGuid();
        var intId = _faker.Random.Int(1, 1000);
        var stringId = _faker.Random.AlphaNumeric(10);

        // Act
        guidEntity.Id = guidId;
        intEntity.Id = intId;
        stringEntity.Id = stringId;

        // Assert
        guidEntity.Id.Should().Be(guidId);
        intEntity.Id.Should().Be(intId);
        stringEntity.Id.Should().Be(stringId);
    }

    [Fact]
    public void Entity_Should_HaveIndependentInstances_When_MultipleEntitiesCreated()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var created1 = DateTime.UtcNow;
        var created2 = DateTime.UtcNow.AddMinutes(1);

        entity1.Id = id1;
        entity1.Created = created1;
        entity2.Id = id2;
        entity2.Created = created2;

        // Assert
        entity1.Id.Should().Be(id1);
        entity1.Created.Should().Be(created1);
        entity2.Id.Should().Be(id2);
        entity2.Created.Should().Be(created2);

        entity1.Id.Should().NotBe(entity2.Id);
        entity1.Created.Should().NotBe(entity2.Created);
    }

    [Fact]
    public void Entity_Should_HandleNullableIdTypes_When_UsingNullableTypes()
    {
        // Arrange & Act
        var nullableIntEntity = new TestEntityWithNullableInt();

        // Assert
        nullableIntEntity.Id.Should().BeNull();

        // Act - Set value
        nullableIntEntity.Id = 42;

        // Assert
        nullableIntEntity.Id.Should().Be(42);

        // Act - Set back to null
        nullableIntEntity.Id = null;

        // Assert
        nullableIntEntity.Id.Should().BeNull();
    }

    [Fact]
    public void Entity_Should_AllowTimestampTracking_When_CreatedAndUpdatedAreSet()
    {
        // Arrange
        var entity = new TestEntity();
        var created = DateTime.UtcNow;
        var updated = DateTime.UtcNow.AddMinutes(5);

        // Act
        entity.Created = created;
        entity.Updated = updated;

        // Assert
        entity.Created.Should().Be(created);
        entity.Updated.Should().Be(updated);
        entity.Updated.Should().BeAfter(entity.Created);
    }

    [Fact]
    public void Entity_Should_AllowSameCreatedAndUpdatedTime_When_TimestampsAreEqual()
    {
        // Arrange
        var entity = new TestEntity();
        var timestamp = DateTime.UtcNow;

        // Act
        entity.Created = timestamp;
        entity.Updated = timestamp;

        // Assert
        entity.Created.Should().Be(timestamp);
        entity.Updated.Should().Be(timestamp);
        entity.Created.Should().Be(entity.Updated);
    }

    [Fact]
    public void Entity_Should_BeReferenceType_When_Instantiated()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = entity1;

        var id = Guid.NewGuid();
        entity2.Id = id;

        // Assert
        entity1.Id.Should().Be(id);
        entity2.Id.Should().Be(id);
        ReferenceEquals(entity1, entity2).Should().BeTrue();
    }

    // Test entity implementations
    private class TestEntity : Entity<Guid>
    {
    }

    private class TestEntityWithGuid : Entity<Guid>
    {
    }

    private class TestEntityWithInt : Entity<int>
    {
    }

    private class TestEntityWithString : Entity<string>
    {
    }

    private class TestEntityWithNullableInt : Entity<int?>
    {
    }
}
