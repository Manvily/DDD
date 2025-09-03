using Analytics.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Domain.Core;

namespace Analytics.Application.Abstractions;

public abstract class BaseAnalyticsEventProcessor(ILogger logger,
    IAnalyticsEventCommandRepository analyticsEventCommandRepository)
{
    protected async Task StoreEventAsync(IDomainEvent domainEvent)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string? errorMessage = null;

        try
        {
            logger.LogInformation("Processing analytics event: {EventType} with ID: {DomainEventId}",
                domainEvent.EventType, domainEvent.EventId.ToString());

            // Store event in MongoDB
            await analyticsEventCommandRepository.StoreEventAsync(domainEvent, stopwatch.Elapsed);

            logger.LogInformation("Successfully processed {EventType} event: {DomainEventId} in {ProcessingTime}ms",
                domainEvent.EventType, domainEvent.EventId.ToString(), stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            logger.LogError(ex, "Failed to process {EventType} event: {DomainEventId}",
                domainEvent.EventType, domainEvent.EventId.ToString());

            // Store failed event for retry mechanism
            try
            {
                await analyticsEventCommandRepository.StoreEventAsync(domainEvent, stopwatch.Elapsed, errorMessage);
            }
            catch (Exception storeEx)
            {
                logger.LogError(storeEx, "Failed to store failed event {EventType} with ID {DomainEventId} for retry",
                    domainEvent.EventType, domainEvent.EventId.ToString());
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
