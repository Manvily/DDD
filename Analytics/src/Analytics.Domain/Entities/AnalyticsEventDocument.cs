using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Analytics.Domain.Entities;

public class AnalyticsEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("aggregateId")]
    public string AggregateId { get; set; } = string.Empty;

    [BsonElement("aggregateType")]
    public string AggregateType { get; set; } = string.Empty;

    [BsonElement("version")]
    [BsonSerializer(typeof(VersionSerializer))]
    public string Version { get; set; } = string.Empty;

    [BsonElement("occurredOn")]
    public DateTime OccurredOn { get; set; }

    [BsonElement("data")]
    public string Data { get; set; } = string.Empty;
}

public class VersionSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        switch (bsonType)
        {
            case BsonType.String:
                return context.Reader.ReadString();
            case BsonType.Double:
                return context.Reader.ReadDouble().ToString();
            case BsonType.Int32:
                return context.Reader.ReadInt32().ToString();
            case BsonType.Int64:
                return context.Reader.ReadInt64().ToString();
            default:
                return string.Empty;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        context.Writer.WriteString(value ?? string.Empty);
    }
}
