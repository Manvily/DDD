using MediatR;

namespace Shared.Domain.Core;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    Guid AggregateId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
    string AggregateType { get; }
    string Source { get; }
    string Version { get; }
    object GetPayload();
}