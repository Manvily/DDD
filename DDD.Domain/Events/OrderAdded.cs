namespace DDD.Domain.Events;

public class OrderAdded : IDomainEvent
{
    public Guid CustomerId { get; set; }
}