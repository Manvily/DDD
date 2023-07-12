using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Order : Entity<Guid>
    {
        public Customer Customer { get; }
        public DateTimeOffset OrderDate { get; }
        public IEnumerable<Product> Products { get; }
        public PaymentStatus Payment { get; set; }

        public Order(Customer customer, DateTimeOffset orderDate, IEnumerable<Product> products, PaymentStatus payment)
        {
            Customer = customer;
            OrderDate = orderDate;
            Products = products;
            Payment = payment;
        }

        public Order()
        {
        }
    }
}
