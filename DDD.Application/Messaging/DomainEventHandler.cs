using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Core;
using Shared.Domain.Interfaces;
using Shared.Domain.Events;

namespace DDD.Application.Messaging
{
    public class DomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<DomainEventHandler> logger
    ) : INotificationHandler<CustomerCreatedEvent>,
        INotificationHandler<OrderCreatedEvent>
    {
        public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
        {
            await PublishEventAsync(notification, cancellationToken);
        }

        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            await PublishEventAsync(notification, cancellationToken);
        }

        private async Task PublishEventAsync(IDomainEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Publishing domain event {EventType} with ID {DomainEventId} to RabbitMQ",
                    notification.EventType, notification.EventId.ToString());

                await eventPublisher.PublishAsync(notification, "analytics.events", "analytics.raw");

                logger.LogInformation("Successfully published domain event {EventType} with ID {DomainEventId} to RabbitMQ",
                    notification.EventType, notification.EventId.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish domain event {EventType} with ID {DomainEventId} to RabbitMQ",
                    notification.EventType, notification.EventId.ToString());
                throw;
            }
        }
    }
}