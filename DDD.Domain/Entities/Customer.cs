using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Customer : Entity<Guid>
    {
        public CustomerName Name { get; }
        public Contact Contact { get; }
        public Address Address { get; }
        public DateTime BirthDate { get; }

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
