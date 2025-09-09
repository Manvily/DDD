using System.Text;
using System.Text.Json;
using AutoFixture;
using Bogus;
using DDD.Infrastructure.Cache;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace DDD.Infrastructure.Tests.Cache;

public class RedisCacheTests
{
    private readonly IDistributedCache _distributedCache;
    private readonly RedisCache _redisCache;
    private readonly IFixture _fixture;
    private readonly Faker _faker;

    public RedisCacheTests()
    {
        _distributedCache = Substitute.For<IDistributedCache>();
        _redisCache = new RedisCache(_distributedCache);
        _fixture = new Fixture();
        _faker = new Faker();
    }

    [Fact]
    public async Task GetAsync_Should_ReturnCachedValue_When_KeyExists()
    {
        // Arrange
        var key = _faker.Random.AlphaNumeric(10);
        var expectedValue = new TestData { Id = Guid.NewGuid(), Name = _faker.Name.FullName() };
        var serializedValue = JsonSerializer.Serialize(expectedValue, new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        });
        var cachedBytes = Encoding.UTF8.GetBytes(serializedValue);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(cachedBytes);

        // Act
        var result = await _redisCache.GetAsync(key, () => Task.FromResult(new TestData()));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedValue.Id);
        result.Name.Should().Be(expectedValue.Name);
        await _distributedCache.Received(1).GetAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_ThrowInvalidOperationException_When_DeserializationReturnsNull()
    {
        // Arrange
        var key = _faker.Random.AlphaNumeric(10);
        var invalidJson = "null";
        var cachedBytes = Encoding.UTF8.GetBytes(invalidJson);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(cachedBytes);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _redisCache.GetAsync(key, () => Task.FromResult(new TestData())));
    }

    [Fact]
    public async Task GetAsync_Should_HandleComplexObjects_When_SerializingAndDeserializing()
    {
        // Arrange
        var key = _faker.Random.AlphaNumeric(10);
        var complexObject = new ComplexTestData
        {
            Id = Guid.NewGuid(),
            Name = _faker.Name.FullName(),
            Items = new List<string> { "Item1", "Item2", "Item3" },
            Properties = new Dictionary<string, object>
            {
                { "Property1", "Value1" },
                { "Property2", 42 },
                { "Property3", true }
            },
            CreatedAt = DateTime.UtcNow
        };

        var serializedValue = JsonSerializer.Serialize(complexObject, new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        });
        var cachedBytes = Encoding.UTF8.GetBytes(serializedValue);

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(cachedBytes);

        // Act
        var result = await _redisCache.GetAsync(key, () => Task.FromResult(new ComplexTestData()));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(complexObject.Id);
        result.Name.Should().Be(complexObject.Name);
        result.Items.Should().BeEquivalentTo(complexObject.Items);
        result.CreatedAt.Should().BeCloseTo(complexObject.CreatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RemoveAsync_Should_CallDistributedCacheRemove_When_Called()
    {
        // Arrange
        var key = _faker.Random.AlphaNumeric(10);

        // Act
        await _redisCache.RemoveAsync(key);

        // Assert
        await _distributedCache.Received(1).RemoveAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_HandleEmptyString_When_KeyIsEmpty()
    {
        // Arrange
        var key = string.Empty;
        var expectedValue = new TestData { Id = Guid.NewGuid(), Name = _faker.Name.FullName() };

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        // Act
        var result = await _redisCache.GetAsync(key, () => Task.FromResult(expectedValue));

        // Assert
        result.Should().Be(expectedValue);
        await _distributedCache.Received(1).GetAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_HandleSpecialCharactersInKey_When_KeyContainsSpecialChars()
    {
        // Arrange
        var key = "special:key-with_chars.and@symbols";
        var expectedValue = new TestData { Id = Guid.NewGuid(), Name = _faker.Name.FullName() };

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        // Act
        var result = await _redisCache.GetAsync(key, () => Task.FromResult(expectedValue));

        // Assert
        result.Should().Be(expectedValue);
        await _distributedCache.Received(1).GetAsync(key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_HandleConcurrentAccess_When_MultipleCallsWithSameKey()
    {
        // Arrange
        var key = _faker.Random.AlphaNumeric(10);
        var expectedValue = new TestData { Id = Guid.NewGuid(), Name = _faker.Name.FullName() };
        var callCount = 0;

        _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns((byte[]?)null);

        Func<Task<TestData>> func = () =>
        {
            Interlocked.Increment(ref callCount);
            return Task.FromResult(expectedValue);
        };

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _redisCache.GetAsync(key, func))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.Should().Be(expectedValue));
        callCount.Should().Be(5); // Each call should invoke the function since cache is empty
    }

    // Test data classes
    public class TestData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            if (obj is TestData other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

    public class ComplexTestData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
