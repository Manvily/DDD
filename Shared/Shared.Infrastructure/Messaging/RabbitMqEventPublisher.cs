using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Shared.Domain.Core;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Messaging;

public class RabbitMqEventPublisher(RabbitMqConnection rabbitMqConnection,
        ILogger<RabbitMqEventPublisher> logger)
    : IEventPublisher, IDisposable
{
    private readonly IModel _channel = rabbitMqConnection.CreateChannel();

    public async Task PublishAsync(IDomainEvent @event, string exchangeName, string routingKey)
    {
        try
        {
            if (!RabbitMqTopologyInitializer.Exchanges.TryGetValue(exchangeName, out _))
            {
                logger.LogError("Exchange {ExchangeName} not found. Declaring it now.", exchangeName);
                throw new InvalidOperationException($"Exchange '{exchangeName}' is not configured.");
            }
            
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                { "EventType", @event.EventType },
                { "Source", @event.Source },
                { "Version", @event.Version }
            };

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            logger.LogInformation("Event {EventType} with ID {DomainEventId} published to RabbitMQ with routing key: {RoutingKey}", 
                @event.EventType, @event.EventId, routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event {EventType} with ID {DomainEventId}", 
                @event.EventType, @event.EventId);
            throw;
        }
    }

    public Task PublishAsync(IEnumerable<IDomainEvent> events)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}