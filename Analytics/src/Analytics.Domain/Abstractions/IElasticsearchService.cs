namespace Analytics.Domain.Abstractions;

public interface IElasticsearchService
{
    Task<bool> IndexDocumentAsync<T>(T document, string? indexName = null) where T : class;
    Task<bool> IndexRabbitMQEventAsync(string eventType, string eventData, string? correlationId = null);
    Task<bool> IndexDomainEventAsync(string eventType, object eventData, string? correlationId = null);
    Task<bool> IndexAnalyticsEventAsync(string eventType, object eventData, string? correlationId = null);
}
