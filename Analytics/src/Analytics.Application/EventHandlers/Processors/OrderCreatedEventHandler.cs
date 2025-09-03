using Analytics.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;

namespace Analytics.Application.EventHandlers.Processors
{
    public class OrderCreatedEventHandler(IMediator mediator,
            ILogger<OrderCreatedEventHandler> logger)
        : INotificationHandler<OrderCreatedEvent>
    {
        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing OrderCreatedEvent: {DomainEventId}", notification.EventId.ToString());

            var command = new StoreAnalyticsEventCommand(notification);
            await mediator.Send(command, cancellationToken);
        }
    }
}
