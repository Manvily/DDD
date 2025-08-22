using Shared.Domain.Core;

namespace Shared.Domain.Interfaces
{
    public interface IEventConsumer
    {
        void InitializeConnection(string queueName, string exchangeName, string routingKey);
        void StartConsuming();
        void StopConsuming();
        void Subscribe<T>(Func<T, Task> handler) where T : IDomainEvent;
    }
}
