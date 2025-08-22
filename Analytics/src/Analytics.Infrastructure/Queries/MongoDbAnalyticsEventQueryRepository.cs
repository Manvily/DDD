using System.Text.Json;
using Analytics.Application.Abstractions;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.MongoDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Analytics.Infrastructure.Queries;

public class MongoDbAnalyticsEventQueryRepository : IAnalyticsEventQueryRepository
{
    private readonly IMongoCollection<AnalyticsEventDocument> _collection;
    private readonly ILogger<MongoDbAnalyticsEventQueryRepository> _logger;

    public MongoDbAnalyticsEventQueryRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings,
        ILogger<MongoDbAnalyticsEventQueryRepository> logger)
    {
        _logger = logger;
        _collection = database.GetCollection<AnalyticsEventDocument>(settings.Value.AnalyticsEventsCollectionName);

        // Create indexes for better performance
        CreateIndexesAsync().Wait();
    }

    public async Task<IEnumerable<object>> GetEventsByTypeAsync(string eventType, DateTime from, DateTime to)
    {
        var filter = Builders<AnalyticsEventDocument>.Filter.And(
            Builders<AnalyticsEventDocument>.Filter.Eq(x => x.EventType, eventType),
            Builders<AnalyticsEventDocument>.Filter.Gte(x => x.OccurredOn, from),
            Builders<AnalyticsEventDocument>.Filter.Lte(x => x.OccurredOn, to)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.OccurredOn)
            .Limit(1000)
            .ToListAsync();

        return documents.Select(doc =>
        {
            try
            {
                return JsonSerializer.Deserialize<object>(doc.Data) ?? new { };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize event data for event {EventId}", doc.EventId);
                return new { };
            }
        });
    }

    public async Task<IEnumerable<object>> GetEventsByAggregateIdAsync(string aggregateId)
    {
        var filter = Builders<AnalyticsEventDocument>.Filter.Eq(x => x.AggregateId, aggregateId);

        var documents = await _collection.Find(filter)
            .SortBy(x => x.Version)
            .ToListAsync();

        return documents.Select(doc =>
        {
            try
            {
                return JsonSerializer.Deserialize<object>(doc.Data) ?? new { };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize event data for event {EventId}", doc.EventId);
                return new { };
            }
        });
    }

    public async Task<long> GetEventsCountAsync(DateTime from, DateTime to)
    {
        var filter = Builders<AnalyticsEventDocument>.Filter.And(
            Builders<AnalyticsEventDocument>.Filter.Gte(x => x.OccurredOn, from),
            Builders<AnalyticsEventDocument>.Filter.Lte(x => x.OccurredOn, to)
        );

        return await _collection.CountDocumentsAsync(filter);
    }

    public async Task<IEnumerable<CustomerEventData>> GetCustomerEventsAsync(DateTime from, DateTime to)
    {
        var filter = Builders<AnalyticsEventDocument>.Filter.And(
            Builders<AnalyticsEventDocument>.Filter.Eq(x => x.EventType, "CustomerCreatedEvent"),
            Builders<AnalyticsEventDocument>.Filter.Gte(x => x.OccurredOn, from),
            Builders<AnalyticsEventDocument>.Filter.Lte(x => x.OccurredOn, to)
        );

        var documents = await _collection.Find(_ => true)
            .SortByDescending(x => x.OccurredOn)
            .ToListAsync();

        return documents.Select(x => new CustomerEventData(x.EventId, x.Data, "aaa", x.OccurredOn, x.EventType));
    }

    public async Task<IEnumerable<OrderEventData>> GetOrderEventsAsync(DateTime from, DateTime to)
    {
        var filter = Builders<AnalyticsEventDocument>.Filter.And(
            Builders<AnalyticsEventDocument>.Filter.Eq(x => x.EventType, "OrderCreatedEvent"),
            Builders<AnalyticsEventDocument>.Filter.Gte(x => x.OccurredOn, from),
            Builders<AnalyticsEventDocument>.Filter.Lte(x => x.OccurredOn, to)
        );

        var documents = await _collection.Find(filter)
            .SortByDescending(x => x.OccurredOn)
            .ToListAsync();

        return documents.Select(doc => ExtractOrderData(doc)).Where(x => x != null)!;
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            // Create indexes for better query performance
            var indexModels = new CreateIndexModel<AnalyticsEventDocument>[]
            {
                new CreateIndexModel<AnalyticsEventDocument>(
                    Builders<AnalyticsEventDocument>.IndexKeys
                        .Ascending(x => x.EventType)
                        .Ascending(x => x.OccurredOn),
                    new CreateIndexOptions { Name = "IX_EventType_OccurredOn" }
                ),
                new CreateIndexModel<AnalyticsEventDocument>(
                    Builders<AnalyticsEventDocument>.IndexKeys
                        .Ascending(x => x.AggregateId)
                        .Ascending(x => x.Version),
                    new CreateIndexOptions { Name = "IX_AggregateId_Version" }
                ),
                new CreateIndexModel<AnalyticsEventDocument>(
                    Builders<AnalyticsEventDocument>.IndexKeys
                        .Descending(x => x.OccurredOn),
                    new CreateIndexOptions { Name = "IX_OccurredOn_Desc" }
                ),
                new CreateIndexModel<AnalyticsEventDocument>(
                    Builders<AnalyticsEventDocument>.IndexKeys
                        .Ascending(x => x.EventId),
                    new CreateIndexOptions { Name = "IX_EventId" }
                )
            };

            await _collection.Indexes.CreateManyAsync(indexModels);

            _logger.LogInformation("Successfully created MongoDB indexes for analytics events collection");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create some MongoDB indexes for analytics events collection");
        }
    }

    private OrderEventData? ExtractOrderData(AnalyticsEventDocument doc)
    {
        try
        {
            if (string.IsNullOrEmpty(doc.Data))
            {
                _logger.LogWarning("Event data is null or empty for event {EventId}", doc.EventId);
                return null;
            }

            var eventData = JsonSerializer.Deserialize<JsonElement>(doc.Data);

            if (eventData.TryGetProperty("CustomerId", out var customerId))
            {
                return new OrderEventData(
                    doc.EventId,
                    customerId.GetString() ?? string.Empty,
                    doc.OccurredOn,
                    doc.EventType
                );
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize order event data for event {EventId}", doc.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract order data from event {EventId}", doc.EventId);
        }

        return null;
    }
}
