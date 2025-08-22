using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;

namespace Analytics.Infrastructure.Consumers
{
    // TODO Learn: jak działa ten backgroundService wraz z AddHostedService
    public class AnalyticsEventConsumer(IEventConsumer eventConsumer,
            ILogger<AnalyticsEventConsumer> logger,
            IServiceProvider serviceProvider)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Starting Analytics Event Consumer Service");

                using var scope = serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                // Subscribe to specific domain events
                eventConsumer.Subscribe<CustomerCreatedEvent>(async (domainEvent) =>
                {
                    await mediator.Publish(domainEvent, stoppingToken);
                });

                eventConsumer.Subscribe<OrderCreatedEvent>(async (domainEvent) =>
                {
                    await mediator.Publish(domainEvent, stoppingToken);
                });

                // Start consuming messages
                eventConsumer.InitializeConnection("analytics_raw", "analytics.events", "analytics.raw");
                eventConsumer.StartConsuming();

                logger.LogInformation("Analytics Event Consumer Service started successfully");

                // Handle cancellation
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Analytics Event Consumer Service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            eventConsumer.StopConsuming();
            logger.LogInformation("Analytics Event Consumer Service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}
