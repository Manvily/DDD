using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Analytics.Domain.Abstractions;
using Analytics.Infrastructure.Configuration;
using System.Text.Json;

namespace Analytics.Infrastructure.Services;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticLowLevelClient _client;
    private readonly ElasticsearchSettings _settings;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly string _serviceName;

    public ElasticsearchService(
        IOptions<ElasticsearchSettings> settings,
        ILogger<ElasticsearchService> logger,
        string serviceName = "Unknown-Service")
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceName = serviceName;

        var config = new ConnectionConfiguration(new Uri(_settings.Url))
            .RequestTimeout(TimeSpan.FromSeconds(60))
            .DeadTimeout(TimeSpan.FromSeconds(60))
            .MaxRetryTimeout(TimeSpan.FromSeconds(60))
            .EnableDebugMode()
            .PrettyJson();

        _client = new ElasticLowLevelClient(config);
    }

    public async Task<bool> IndexDocumentAsync<T>(T document, string? indexName = null) where T : class
    {
        try
        {
            var index = indexName ?? _settings.DefaultIndex;
            var json = JsonSerializer.Serialize(document);

            var response = await _client.IndexAsync<StringResponse>(index, json);

            if (response.Success)
            {
                _logger.LogInformation("Document indexed successfully in {Index}", index);
                return true;
            }

            _logger.LogError("Failed to index document in {Index}: {Error}", index, response.Body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document");
            return false;
        }
    }

    public async Task<bool> IndexRabbitMQEventAsync(string eventType, string eventData, string? correlationId = null)
    {
        var document = new
        {
            ServiceName = _serviceName,
            EventType = eventType,
            EventData = eventData,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        return await IndexDocumentAsync(document, "rabbitmq-events");
    }

    public async Task<bool> IndexDomainEventAsync(string eventType, object eventData, string? correlationId = null)
    {
        var document = new
        {
            ServiceName = _serviceName,
            EventType = eventType,
            EventData = JsonSerializer.Serialize(eventData),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        return await IndexDocumentAsync(document, "domain-events");
    }

    public async Task<bool> IndexAnalyticsEventAsync(string eventType, object eventData, string? correlationId = null)
    {
        var document = new
        {
            ServiceName = _serviceName,
            EventType = eventType,
            EventData = JsonSerializer.Serialize(eventData),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        return await IndexDocumentAsync(document, "analytics-events");
    }
}
