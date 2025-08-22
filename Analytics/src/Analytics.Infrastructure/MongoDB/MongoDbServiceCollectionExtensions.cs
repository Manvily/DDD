using Analytics.Application.Abstractions;
using Analytics.Infrastructure.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Analytics.Infrastructure.MongoDB;

public static class MongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB serialization settings
        ConfigureMongoDbSerialization();

        return services;
    }

    private static void ConfigureMongoDbSerialization()
    {
        // Configure MongoDB to handle string serialization properly
        BsonSerializer.RegisterSerializer(typeof(string), new StringSerializer(BsonType.String));

        // Configure MongoDB to handle DateTime serialization
        BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Utc));
    }
}
