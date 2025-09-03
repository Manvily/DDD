using Analytics.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;

namespace Analytics.Application.EventHandlers.Processors
{
    public class CustomerCreatedEventHandler(IMediator mediator,
            ILogger<CustomerCreatedEventHandler> logger)
        : INotificationHandler<CustomerCreatedEvent>
    {
        public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing CustomerCreatedEvent: {DomainEventId}", notification.EventId.ToString());

            var command = new StoreAnalyticsEventCommand(notification);
            await mediator.Send(command, cancellationToken);
        }
    }
}
