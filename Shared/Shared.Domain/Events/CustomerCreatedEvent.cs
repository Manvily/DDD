namespace Shared.Domain.Events;

/// <summary>
/// Event published when a new customer is created in the system.
/// This event can be used for various purposes including analytics, notifications, and customer lifecycle management.
/// </summary>
/// <remarks>
/// This event is published by the main API when a customer registration is completed.
/// </remarks>
public class CustomerCreatedEvent : BaseDomainEvent
{
    /// <summary>
    /// Unique identifier of the newly created customer.
    /// This ID is used to track the customer across all systems and services.
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid CustomerId { get; }
    
    /// <summary>
    /// Full name of the customer as provided during registration.
    /// This is the display name that will be used in the system.
    /// </summary>
    /// <example>John Doe</example>
    public string CustomerName { get; }

    /// <summary>
    /// Initializes a new instance of the CustomerCreatedEvent class.
    /// </summary>
    /// <param name="customerId">Unique identifier of the customer</param>
    /// <param name="customerName">Full name of the customer</param>
    /// <exception cref="ArgumentException">Thrown when customerId is empty or customerName is null/empty</exception>
    public CustomerCreatedEvent(Guid customerId, string customerName, string source) 
        : base(source, customerId, "Customer", "1.0")
    {
        CustomerId = customerId != Guid.Empty ? customerId : throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
        CustomerName = !string.IsNullOrWhiteSpace(customerName) ? customerName : throw new ArgumentException("CustomerName cannot be empty", nameof(customerName));
    }

    public override object GetPayload() => new
    {
        CustomerId,
        CustomerName,
    };
}