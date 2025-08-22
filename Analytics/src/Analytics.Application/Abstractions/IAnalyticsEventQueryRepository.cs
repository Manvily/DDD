namespace Analytics.Application.Abstractions;

public interface IAnalyticsEventQueryRepository
{
    Task<IEnumerable<object>> GetEventsByTypeAsync(string eventType, DateTime from, DateTime to);
    Task<IEnumerable<object>> GetEventsByAggregateIdAsync(string aggregateId);
    Task<long> GetEventsCountAsync(DateTime from, DateTime to);
    Task<IEnumerable<CustomerEventData>> GetCustomerEventsAsync(DateTime from, DateTime to);
    Task<IEnumerable<OrderEventData>> GetOrderEventsAsync(DateTime from, DateTime to);
}

public record CustomerEventData(
    string EventId,
    string CustomerId,
    string CustomerName,
    DateTime OccurredOn,
    string EventType
);

public record OrderEventData(
    string EventId,
    string CustomerId,
    DateTime OccurredOn,
    string EventType
);
