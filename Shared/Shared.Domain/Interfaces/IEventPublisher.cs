using Shared.Domain.Core;

namespace Shared.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(IDomainEvent @event, string exchangeName, string routingKey);
        Task PublishAsync(IEnumerable<IDomainEvent> events);
    }
}
