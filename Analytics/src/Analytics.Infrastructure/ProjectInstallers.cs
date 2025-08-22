using Analytics.Application.Abstractions;
using Analytics.Application.EventHandlers;
using Analytics.Infrastructure.Commands;
using Analytics.Infrastructure.Consumers;
using Analytics.Infrastructure.MongoDB;
using Analytics.Infrastructure.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Startup;

namespace Analytics.Infrastructure
{
    public static class ProjectInstallers
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add MongoDB services and configuration
            services.AddMongoDbServices(configuration);
            services.AddMongoDb(configuration);

            // Add RabbitMQ messaging
            services.AddRabbitMQMessaging(configuration);

            // Register background services
            services.AddHostedService<AnalyticsEventConsumer>();

            return services;
        }

            public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB settings
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        // Register MongoDB client with best practices
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            var connectionString = settings.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is not configured");
            }

            var mongoUrl = new MongoUrl(connectionString);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);

            // Configure connection pooling
            mongoClientSettings.MaxConnectionPoolSize = settings.MaxPoolSize;
            mongoClientSettings.MinConnectionPoolSize = settings.MinPoolSize;
            mongoClientSettings.MaxConnectionIdleTime = TimeSpan.FromMilliseconds(settings.MaxIdleTimeMs);

            // Configure timeouts
            mongoClientSettings.ConnectTimeout = TimeSpan.FromMilliseconds(settings.ConnectionTimeoutMs);
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(settings.ServerSelectionTimeoutMs);
            mongoClientSettings.SocketTimeout = TimeSpan.FromMilliseconds(settings.SocketTimeoutMs);

            // Configure retry policies
            mongoClientSettings.RetryWrites = settings.RetryWrites;
            mongoClientSettings.RetryReads = settings.RetryReads;

            // Configure heartbeat
            mongoClientSettings.HeartbeatInterval = TimeSpan.FromMilliseconds(settings.HeartbeatIntervalMs);

            // Configure application name for monitoring
            mongoClientSettings.ApplicationName = "Analytics.Service";

            // Configure logging
            mongoClientSettings.LoggingSettings = new LoggingSettings();

            return new MongoClient(mongoClientSettings);
        });

        // Register MongoDB database
        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            var databaseName = settings.DatabaseName;
            if (string.IsNullOrEmpty(databaseName))
            {
                // Extract database name from connection string if not specified
                var connectionString = settings.ConnectionString;
                var mongoUrl = new MongoUrl(connectionString);
                databaseName = mongoUrl.DatabaseName ?? "AnalyticsDb";
            }

            return client.GetDatabase(databaseName);
        });

        // Register repositories (CQRS pattern)
        services.AddScoped<IAnalyticsEventCommandRepository, MongoDbAnalyticsEventCommandRepository>();
        services.AddScoped<IAnalyticsEventQueryRepository, MongoDbAnalyticsEventQueryRepository>();

        // Add health check for MongoDB
        services.AddHealthChecks()
            .AddMongoDb(
                configuration.GetConnectionString("MongoDB") ?? "mongodb://mongo-analytics:27017",
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "database", "mongodb" });

        return services;
    }

    }
}
