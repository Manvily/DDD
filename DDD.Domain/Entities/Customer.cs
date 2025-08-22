using DDD.Domain.ValueObjects;
using Shared.Domain.Core;

namespace DDD.Domain.Entities
{
    public class Customer : Entity<Guid>
    {
        public CustomerName Name { get; set; }
        public Contact Contact { get; set; }
        public Address Address { get; set; }
        public DateTime BirthDate { get; set; }
        public IEnumerable<Order> Orders { get; set; }

        public Customer(CustomerName name, Contact contact, Address address, DateTime birthDate)
        {
            Name = name;
            Contact = contact;
            Address = address;
            BirthDate = birthDate;
        }

        public Customer()
        {
        }
    }
}
