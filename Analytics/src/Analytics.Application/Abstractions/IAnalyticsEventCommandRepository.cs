using Shared.Domain.Core;

namespace Analytics.Application.Abstractions;

public interface IAnalyticsEventCommandRepository
{
    Task StoreEventAsync(IDomainEvent domainEvent, TimeSpan processingDuration, string? errorMessage = null);
}
