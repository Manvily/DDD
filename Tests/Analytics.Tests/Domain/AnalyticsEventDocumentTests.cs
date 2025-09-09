using Analytics.Domain.Entities;
using AutoFixture;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Analytics.Tests.Domain;

public class AnalyticsEventDocumentTests
{
    private readonly IFixture _fixture;
    private readonly Faker _faker;

    public AnalyticsEventDocumentTests()
    {
        _fixture = new Fixture();
        _faker = new Faker();
    }

    [Fact]
    public void AnalyticsEventDocument_Should_HaveCorrectDefaultValues_When_Instantiated()
    {
        // Act
        var document = new AnalyticsEventDocument();

        // Assert
        document.Id.Should().Be(string.Empty);
        document.EventId.Should().Be(string.Empty);
        document.EventType.Should().Be(string.Empty);
        document.AggregateId.Should().Be(string.Empty);
        document.AggregateType.Should().Be(string.Empty);
        document.Version.Should().Be(string.Empty);
        document.OccurredOn.Should().Be(default(DateTime));
        document.Data.Should().Be(string.Empty);
    }

    [Fact]
    public void AnalyticsEventDocument_Should_AllowSettingAllProperties_When_PropertiesAreSet()
    {
        // Arrange
        var document = new AnalyticsEventDocument();
        var id = ObjectId.GenerateNewId().ToString();
        var eventId = Guid.NewGuid().ToString();
        var eventType = "CustomerCreatedEvent";
        var aggregateId = Guid.NewGuid().ToString();
        var aggregateType = "Customer";
        var version = "1.0";
        var occurredOn = DateTime.UtcNow;
        var data = "{\"customerId\":\"123\",\"customerName\":\"John Doe\"}";

        // Act
        document.Id = id;
        document.EventId = eventId;
        document.EventType = eventType;
        document.AggregateId = aggregateId;
        document.AggregateType = aggregateType;
        document.Version = version;
        document.OccurredOn = occurredOn;
        document.Data = data;

        // Assert
        document.Id.Should().Be(id);
        document.EventId.Should().Be(eventId);
        document.EventType.Should().Be(eventType);
        document.AggregateId.Should().Be(aggregateId);
        document.AggregateType.Should().Be(aggregateType);
        document.Version.Should().Be(version);
        document.OccurredOn.Should().Be(occurredOn);
        document.Data.Should().Be(data);
    }

    [Fact]
    public void AnalyticsEventDocument_Should_HandleNullValues_When_PropertiesAreSetToNull()
    {
        // Arrange
        var document = new AnalyticsEventDocument();

        // Act
        document.Id = null!;
        document.EventId = null!;
        document.EventType = null!;
        document.AggregateId = null!;
        document.AggregateType = null!;
        document.Version = null!;
        document.Data = null!;

        // Assert
        document.Id.Should().BeNull();
        document.EventId.Should().BeNull();
        document.EventType.Should().BeNull();
        document.AggregateId.Should().BeNull();
        document.AggregateType.Should().BeNull();
        document.Version.Should().BeNull();
        document.Data.Should().BeNull();
    }

    [Fact]
    public void AnalyticsEventDocument_Should_HandleLongStrings_When_PropertiesAreVeryLong()
    {
        // Arrange
        var document = new AnalyticsEventDocument();
        var longString = new string('A', 10000);
        var longJsonData = "{\"property\":\"" + new string('B', 5000) + "\"}";

        // Act
        document.EventType = longString;
        document.Data = longJsonData;

        // Assert
        document.EventType.Should().Be(longString);
        document.Data.Should().Be(longJsonData);
    }

    [Fact]
    public void AnalyticsEventDocument_Should_HandleSpecialCharacters_When_PropertiesContainSpecialChars()
    {
        // Arrange
        var document = new AnalyticsEventDocument();
        var specialCharsString = "Event-Type_With.Special@Characters#123!";
        var jsonWithSpecialChars = "{\"name\":\"José María\",\"symbol\":\"@#$%^&*()\"}";

        // Act
        document.EventType = specialCharsString;
        document.Data = jsonWithSpecialChars;

        // Assert
        document.EventType.Should().Be(specialCharsString);
        document.Data.Should().Be(jsonWithSpecialChars);
    }

    [Fact]
    public void AnalyticsEventDocument_Should_HandleDifferentDateTimes_When_OccurredOnIsSet()
    {
        // Arrange
        var document = new AnalyticsEventDocument();
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(30);
        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;

        // Act & Assert
        document.OccurredOn = pastDate;
        document.OccurredOn.Should().Be(pastDate);

        document.OccurredOn = futureDate;
        document.OccurredOn.Should().Be(futureDate);

        document.OccurredOn = minDate;
        document.OccurredOn.Should().Be(minDate);

        document.OccurredOn = maxDate;
        document.OccurredOn.Should().Be(maxDate);
    }

    [Fact]
    public void AnalyticsEventDocument_Should_BeIndependent_When_MultipleInstancesCreated()
    {
        // Arrange & Act
        var document1 = new AnalyticsEventDocument
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "Event1",
            Data = "Data1"
        };

        var document2 = new AnalyticsEventDocument
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = "Event2",
            Data = "Data2"
        };

        // Assert
        document1.EventId.Should().NotBe(document2.EventId);
        document1.EventType.Should().NotBe(document2.EventType);
        document1.Data.Should().NotBe(document2.Data);
    }
}

public class VersionSerializerTests
{
    private readonly VersionSerializer _serializer;

    public VersionSerializerTests()
    {
        _serializer = new VersionSerializer();
    }

    [Fact]
    public void Serialize_Should_WriteStringValue_When_ValueIsString()
    {
        // Arrange
        var value = "1.0.0";
        var document = new BsonDocument();
        using var writer = new BsonDocumentWriter(document);
        var context = BsonSerializationContext.CreateRoot(writer);
        var args = new BsonSerializationArgs();

        // Act
        writer.WriteStartDocument();
        writer.WriteName("version");
        _serializer.Serialize(context, args, value);
        writer.WriteEndDocument();

        // Assert
        document["version"].AsString.Should().Be(value);
    }

    [Fact]
    public void Serialize_Should_WriteEmptyString_When_ValueIsNull()
    {
        // Arrange
        string? value = null;
        var document = new BsonDocument();
        using var writer = new BsonDocumentWriter(document);
        var context = BsonSerializationContext.CreateRoot(writer);
        var args = new BsonSerializationArgs();

        // Act
        writer.WriteStartDocument();
        writer.WriteName("version");
        _serializer.Serialize(context, args, value!);
        writer.WriteEndDocument();

        // Assert
        document["version"].AsString.Should().Be(string.Empty);
    }

    [Fact]
    public void Deserialize_Should_ReturnString_When_BsonTypeIsString()
    {
        // Arrange
        var expectedValue = "2.1.0";
        var document = new BsonDocument { { "version", expectedValue } };
        using var reader = new BsonDocumentReader(document);
        var context = BsonDeserializationContext.CreateRoot(reader);
        var args = new BsonDeserializationArgs();

        // Act
        reader.ReadStartDocument();
        reader.ReadName();
        var result = _serializer.Deserialize(context, args);
        reader.ReadEndDocument();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void Deserialize_Should_ReturnStringFromDouble_When_BsonTypeIsDouble()
    {
        // Arrange
        var doubleValue = 1.5;
        var document = new BsonDocument { { "version", doubleValue } };
        using var reader = new BsonDocumentReader(document);
        var context = BsonDeserializationContext.CreateRoot(reader);
        var args = new BsonDeserializationArgs();

        // Act
        reader.ReadStartDocument();
        reader.ReadName();
        var result = _serializer.Deserialize(context, args);
        reader.ReadEndDocument();

        // Assert
        result.Should().Be(doubleValue.ToString());
    }

    [Fact]
    public void Deserialize_Should_ReturnStringFromInt32_When_BsonTypeIsInt32()
    {
        // Arrange
        var intValue = 42;
        var document = new BsonDocument { { "version", intValue } };
        using var reader = new BsonDocumentReader(document);
        var context = BsonDeserializationContext.CreateRoot(reader);
        var args = new BsonDeserializationArgs();

        // Act
        reader.ReadStartDocument();
        reader.ReadName();
        var result = _serializer.Deserialize(context, args);
        reader.ReadEndDocument();

        // Assert
        result.Should().Be(intValue.ToString());
    }

    [Fact]
    public void Deserialize_Should_ReturnStringFromInt64_When_BsonTypeIsInt64()
    {
        // Arrange
        var longValue = 9223372036854775807L;
        var document = new BsonDocument { { "version", longValue } };
        using var reader = new BsonDocumentReader(document);
        var context = BsonDeserializationContext.CreateRoot(reader);
        var args = new BsonDeserializationArgs();

        // Act
        reader.ReadStartDocument();
        reader.ReadName();
        var result = _serializer.Deserialize(context, args);
        reader.ReadEndDocument();

        // Assert
        result.Should().Be(longValue.ToString());
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("2.1.0")]
    [InlineData("1.0.0-beta")]
    [InlineData("3.2.1-rc.1")]
    public void Serialize_And_Deserialize_Should_RoundTrip_When_UsingStringValues(string version)
    {
        // Arrange
        var document = new BsonDocument();
        using var writer = new BsonDocumentWriter(document);
        var serializationContext = BsonSerializationContext.CreateRoot(writer);
        var args = new BsonSerializationArgs();

        // Act - Serialize
        writer.WriteStartDocument();
        writer.WriteName("version");
        _serializer.Serialize(serializationContext, args, version);
        writer.WriteEndDocument();

        // Act - Deserialize
        using var reader = new BsonDocumentReader(document);
        var deserializationContext = BsonDeserializationContext.CreateRoot(reader);
        reader.ReadStartDocument();
        reader.ReadName();
        var deserializationArgs = new BsonDeserializationArgs { NominalType = typeof(string) };
        var result = _serializer.Deserialize(deserializationContext, deserializationArgs);
        reader.ReadEndDocument();

        // Assert
        result.Should().Be(version);
    }
}
