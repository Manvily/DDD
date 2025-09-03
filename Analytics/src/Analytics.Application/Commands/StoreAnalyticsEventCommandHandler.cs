using Analytics.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Commands;

public class StoreAnalyticsEventCommandHandler(IAnalyticsEventCommandRepository commandRepository,
        ILogger<StoreAnalyticsEventCommandHandler> logger)
    : IRequestHandler<StoreAnalyticsEventCommand, Unit>
{
    public async Task<Unit> Handle(StoreAnalyticsEventCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Storing analytics event: {EventType} with ID: {DomainEventId}",
                request.DomainEvent.EventType, request.DomainEvent.EventId.ToString());

            await commandRepository.StoreEventAsync(request.DomainEvent, stopwatch.Elapsed);

            logger.LogInformation("Successfully stored {EventType} event: {DomainEventId} in {ProcessingTime}ms",
                request.DomainEvent.EventType, request.DomainEvent.EventId.ToString(), stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to store {EventType} event: {DomainEventId}",
                request.DomainEvent.EventType, request.DomainEvent.EventId.ToString());

            // Store failed event for retry mechanism
            try
            {
                await commandRepository.StoreEventAsync(request.DomainEvent, stopwatch.Elapsed, ex.Message);
            }
            catch (Exception storeEx)
            {
                logger.LogError(storeEx, "Failed to store failed event {EventType} with ID {DomainEventId} for retry",
                    request.DomainEvent.EventType, request.DomainEvent.EventId.ToString());
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
        }

        return Unit.Value;
    }
}
