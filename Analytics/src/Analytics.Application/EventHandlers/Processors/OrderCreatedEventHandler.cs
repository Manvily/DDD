using Analytics.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;

namespace Analytics.Application.EventHandlers.Processors
{
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(IMediator mediator, ILogger<OrderCreatedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing OrderCreatedEvent: {EventId}", notification.EventId);

            var command = new StoreAnalyticsEventCommand(notification);
            await _mediator.Send(command, cancellationToken);
        }
    }
}
