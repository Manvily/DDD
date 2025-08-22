using Shared.Domain.Core;

namespace Shared.Domain.Events
{
    public abstract class BaseDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public Guid AggregateId { get; }
        public DateTime OccurredOn { get; }
        public string AggregateType { get; }
        public string EventType { get; }
        public string Source { get; }
        public string Version { get; }

        protected BaseDomainEvent(string source, Guid aggregateId, string aggregateType, string version = "1.0")
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventType = GetType().Name;
            Version = version;
            Source = !string.IsNullOrWhiteSpace(source) ? source : throw new ArgumentException("Source cannot be empty", nameof(source));
            AggregateId = aggregateId != Guid.Empty ? aggregateId : throw new ArgumentException("AggregateId cannot be empty", nameof(aggregateId));
            AggregateType = !string.IsNullOrWhiteSpace(aggregateType) ? aggregateType : throw new ArgumentException("AggregateType cannot be empty", nameof(aggregateType));
        }

        public abstract object GetPayload();
    }
}
