namespace Analytics.Application.ViewModels;

public record AnalyticsEventViewModel(
    string EventId,
    string EventType,
    string AggregateId,
    long Version,
    DateTime OccurredOn,
    object Data
);

public record CustomerEventViewModel(
    string EventId,
    string CustomerId,
    string CustomerName,
    DateTime OccurredOn,
    string EventType
);

public record OrderEventViewModel(
    string EventId,
    string CustomerId,
    DateTime OccurredOn,
    string EventType
);

public record EventsCountViewModel(
    long Count,
    DateTime From,
    DateTime To
);
