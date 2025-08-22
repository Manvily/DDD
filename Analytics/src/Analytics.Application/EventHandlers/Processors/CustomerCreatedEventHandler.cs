using Analytics.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;

namespace Analytics.Application.EventHandlers.Processors
{
    public class CustomerCreatedEventHandler : INotificationHandler<CustomerCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerCreatedEventHandler> _logger;

        public CustomerCreatedEventHandler(IMediator mediator, ILogger<CustomerCreatedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing CustomerCreatedEvent: {EventId}", notification.EventId);

            var command = new StoreAnalyticsEventCommand(notification);
            await _mediator.Send(command, cancellationToken);
        }
    }
}
