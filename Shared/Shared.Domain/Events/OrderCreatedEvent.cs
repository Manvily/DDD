namespace Shared.Domain.Events;

/// <summary>
/// Event published when a new order is created in the system.
/// This event can be used for order lifecycle tracking, inventory management, and various business processes.
/// </summary>
/// <remarks>
/// This event is published when an order is initially created, before items are added.
/// </remarks>
public class OrderCreatedEvent : BaseDomainEvent
{
    /// <summary>
    /// Unique identifier of the customer who created the order.
    /// This ID links the order to the customer who placed it.
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid CustomerId { get; }

    /// <summary>
    /// Initializes a new instance of the OrderCreatedEvent class.
    /// </summary>
    /// <param name="customerId">Unique identifier of the customer who created the order</param>
    /// <param name="source">Source system that published this event</param>
    /// <param name="version">Version of the event contract</param>
    /// <exception cref="ArgumentException">Thrown when customerId is empty</exception>
    public OrderCreatedEvent(Guid customerId, string source, Guid orderId) : base(source, orderId, "Order", "1.0")
    {
        CustomerId = customerId != Guid.Empty ? customerId : throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
    }
    
    public override object GetPayload() => new
    {
        CustomerId,
    };
}