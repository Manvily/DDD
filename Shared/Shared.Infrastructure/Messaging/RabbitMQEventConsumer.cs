using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.Domain.Core;
using Shared.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System;
using Shared.Domain.Events;

namespace Shared.Infrastructure.Messaging
{
    public class RabbitMqEventConsumer : IEventConsumer, IDisposable
    {
        private IModel? _channel;
        private readonly RabbitMqConnection _rabbitMqConnection;
        private readonly ILogger<RabbitMqEventConsumer> _logger;
        private readonly ConcurrentDictionary<string, Func<IDomainEvent, Task>> _handlers;
        private readonly ConcurrentDictionary<string, Type> _eventTypes;
        private readonly ConcurrentDictionary<string, int> _messageRetryCount;
        private EventingBasicConsumer? _consumer;
        private bool _isConsuming;
        private readonly object _lockObject = new object();
        private string? _consumerTag;
        private string _queueName;
        private string _exchangeName;
        private string _routingKey;

        public RabbitMqEventConsumer(
            RabbitMqConnection rabbitMqConnection,
            ILogger<RabbitMqEventConsumer> logger)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _logger = logger;
            _handlers = new ConcurrentDictionary<string, Func<IDomainEvent, Task>>();
            _eventTypes = new ConcurrentDictionary<string, Type>();
            _messageRetryCount = new ConcurrentDictionary<string, int>();
        }

        public void InitializeConnection(string queueName, string exchangeName, string routingKey)
        {
            _queueName = queueName;
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            
            var maxRetries = 5;
            var retryDelayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                    _channel = _rabbitMqConnection.CreateChannel();
                    
                    SetupConsumer();
                    
                    _logger.LogInformation("Successfully connected to RabbitMQ");
                    return;
                }
                catch (Exception ex)
                {
                    var isExchangeNotFound = ex.Message.Contains("NOT_FOUND") || ex.Message.Contains("does not exist");
                    
                    if (isExchangeNotFound)
                    {
                        _logger.LogInformation("Exchange not found on attempt {Attempt}/{MaxRetries}. Waiting for publisher to declare it...", attempt, maxRetries);
                    }
                    else
                    {
                        _logger.LogWarning(ex, "Failed to connect to RabbitMQ on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    }
                    
                    if (attempt == maxRetries)
                    {
                        _logger.LogError(ex, "Failed to connect to RabbitMQ after {MaxRetries} attempts. The service will continue without RabbitMQ connectivity.", maxRetries);
                        throw;
                    }
                    
                    // Wait before retrying
                    Thread.Sleep(retryDelayMs);
                    retryDelayMs = Math.Min(retryDelayMs * 2, 10000); // Exponential backoff with max 10 seconds
                }
            }
        }

        private void SetupConsumer()
        {
            if (_channel == null)
            {
                _logger.LogWarning("Cannot setup consumer - channel is null");
                return;
            }

            try
            {
                // Check if the main exchange exists, if not throw an exception to trigger retry
                try
                {
                    _channel.ExchangeDeclarePassive(_exchangeName);
                    _logger.LogInformation("Exchange {ExchangeName} exists, proceeding with queue setup", _exchangeName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Exchange {ExchangeName} does not exist yet. Publisher needs to declare it first. Error: {Error}", 
                        _exchangeName, ex.Message);
                    throw new InvalidOperationException($"Exchange '{_exchangeName}' does not exist. Publisher must declare it first.", ex);
                }

                // Setup dlq
                _channel.ExchangeDeclare($"{_queueName}.dlx", ExchangeType.Direct, durable: true);
                _channel.QueueDeclare($"{_queueName}.dlq", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind($"{_queueName}.dlq", $"{_queueName}.dlx", $"{_queueName}.dlq");
                var args = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", $"{_queueName}.dlx" },
                    { "x-dead-letter-routing-key", $"{_queueName}.dlq" }
                };
                
                // Declare queue with dead letter configuration
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false, 
                    arguments: args);

                // Bind queue to exchange
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: _routingKey);
                
                _logger.LogInformation("Queue {QueueName} declared successfully with dead letter configuration and bound to exchange", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up RabbitMQ consumer");
                throw;
            }

            // Set QoS for fair dispatch
            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false);

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnMessageReceived;
        }

        public void Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent
        {
            var eventType = typeof(T).Name;
            _handlers[eventType] = async (domainEvent) => await handler((T)domainEvent);
            _eventTypes[eventType] = typeof(T);
            
            _logger.LogInformation("Subscribed to event type: {EventType}", eventType);
        }

        public void StartConsuming()
        {
            lock (_lockObject)
            {
                if (_isConsuming || _channel == null || _consumer == null)
                {
                    _logger.LogWarning("Cannot start consuming - already consuming or channel/consumer is null");
                    return;
                }

                try
                {
                    // Log queue information before starting consumption
                    var queueInfo = _channel.QueueDeclarePassive(_queueName);
                    _logger.LogInformation("Queue {QueueName} has {MessageCount} messages and {ConsumerCount} consumers", 
                        _queueName, queueInfo.MessageCount, queueInfo.ConsumerCount);

                    _consumerTag = _channel.BasicConsume(
                        queue: _queueName,
                        autoAck: false,
                        consumer: _consumer);

                    _isConsuming = true;
                    _logger.LogInformation("Started consuming messages from queue: {QueueName} with consumer tag: {ConsumerTag}", 
                        _queueName, _consumerTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting message consumption");
                    throw;
                }
            }
        }

        public void StopConsuming()
        {
            lock (_lockObject)
            {
                if (!_isConsuming || _channel == null)
                {
                    return;
                }

                try
                {
                    if (!string.IsNullOrEmpty(_consumerTag))
                    {
                        _channel.BasicCancel(_consumerTag);
                    }
                    _isConsuming = false;
                    _consumerTag = null;
                    _logger.LogInformation("Stopped consuming messages");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping message consumption");
                }
            }
        }

        private async void OnMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            var messageId = e.BasicProperties.MessageId ?? e.DeliveryTag.ToString();
            
            try
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = e.RoutingKey;

                _logger.LogInformation("Received message with routing key: {RoutingKey}, MessageId: {MessageId}, Body: {MessageBody}", routingKey, messageId, message);

                // First, extract the EventType from the JSON message
                var eventType = ExtractEventTypeFromMessage(message);
                
                if (string.IsNullOrEmpty(eventType))
                {
                    _logger.LogWarning("Could not extract EventType from message");
                    AcknowledgeMessage(e.DeliveryTag, false); // Remove from queue - no handler
                    return;
                }

                _logger.LogDebug("Extracted EventType: {EventType}", eventType);

                // Check if we have a handler for this event type
                if (!_handlers.TryGetValue(eventType, out var handler))
                {
                    _logger.LogWarning("No handler found for event type: {EventType}", eventType);
                    AcknowledgeMessage(e.DeliveryTag, false); // Remove from queue - no handler
                    return;
                }

                // Get the concrete event type class
                if (!_eventTypes.TryGetValue(eventType, out var eventTypeClass))
                {
                    _logger.LogWarning("Event type class not found for: {EventType}", eventType);
                    AcknowledgeMessage(e.DeliveryTag, false); // Remove from queue - no handler
                    return;
                }

                // Deserialize the message to the correct concrete type
                var domainEvent = JsonConvert.DeserializeObject(message, eventTypeClass) as IDomainEvent;
                
                if (domainEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize message to type {EventTypeClass} for event type: {EventType}", 
                        eventTypeClass.Name, eventType);
                    AcknowledgeMessage(e.DeliveryTag, false); // Remove from queue - deserialization failed
                    return;
                }

                // Process the event
                await handler(domainEvent);

                // Success - remove from retry tracking and acknowledge
                _messageRetryCount.TryRemove(messageId, out _);
                AcknowledgeMessage(e.DeliveryTag, false);

                _logger.LogDebug("Successfully processed message for event type: {EventType}", eventType);
            }
            catch (Exception ex)
            {
                // Get current retry count
                var retryCount = _messageRetryCount.GetOrAdd(messageId, 0);
                var maxRetries = 3;
                
                retryCount++;
                _messageRetryCount.AddOrUpdate(messageId, retryCount, (key, oldValue) => retryCount);

                _logger.LogWarning(ex, "Error processing message {MessageId} (attempt {RetryCount}/{MaxRetries})", 
                    messageId, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    // Max retries exceeded - move to dead letter
                    _logger.LogError(ex, "Message {MessageId} exceeded max retries ({MaxRetries}), moving to dead letter queue", 
                        messageId, maxRetries);
                    
                    _messageRetryCount.TryRemove(messageId, out _);
                    AcknowledgeMessage(e.DeliveryTag, false); // BasicAck - RabbitMQ will move to dead letter
                }
                else
                {
                    // Retry - requeue the message
                    AcknowledgeMessage(e.DeliveryTag, true); // BasicNack with requeue=true
                }
            }
        }

        private string ExtractEventTypeFromMessage(string message)
        {
            try
            {
                // Parse the JSON to extract the EventType property
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(message);
                return jsonObject?.EventType?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract EventType from message");
                return null;
            }
        }

        private void AcknowledgeMessage(ulong deliveryTag, bool requeue)
        {
            try
            {
                if (_channel?.IsOpen == true)
                {
                    if (requeue)
                    {
                        _channel.BasicNack(deliveryTag, false, true);
                    }
                    else
                    {
                        _channel.BasicAck(deliveryTag, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging message");
            }
        }

        public void Dispose()
        {
            try
            {
                StopConsuming();
                
                _channel?.Close();
                _channel?.Dispose();

                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
        }
    }
}
