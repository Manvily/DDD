using System.Text.Json;
using Analytics.Application.Abstractions;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.MongoDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Domain.Core;

namespace Analytics.Infrastructure.Commands;

public class MongoDbAnalyticsEventCommandRepository : IAnalyticsEventCommandRepository
{
    private readonly IMongoCollection<AnalyticsEventDocument> _collection;
    private readonly ILogger<MongoDbAnalyticsEventCommandRepository> _logger;

    public MongoDbAnalyticsEventCommandRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings,
        ILogger<MongoDbAnalyticsEventCommandRepository> logger)
    {
        var settings1 = settings.Value;
        _logger = logger;
        _collection = database.GetCollection<AnalyticsEventDocument>(settings1.AnalyticsEventsCollectionName);
    }

    public async Task StoreEventAsync(IDomainEvent domainEvent, TimeSpan processingDuration, string? errorMessage = null)
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        try
        {
            var document = new AnalyticsEventDocument
            {
                EventId = domainEvent.EventId.ToString(),
                EventType = domainEvent.EventType,
                AggregateId = domainEvent.AggregateId.ToString(),
                AggregateType = domainEvent.AggregateType,
                Version = domainEvent.Version,
                OccurredOn = domainEvent.OccurredOn,
                Data = JsonSerializer.Serialize(domainEvent.GetPayload(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                }) ?? "{}",
            };

            await _collection.InsertOneAsync(document);

            _logger.LogInformation("Successfully stored analytics event {EventType} with ID {DomainEventId}",
                domainEvent.EventType, domainEvent.EventId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store analytics event {EventType} with ID {DomainEventId}",
                domainEvent.EventType, domainEvent.EventId.ToString());
            throw;
        }
    }
}
