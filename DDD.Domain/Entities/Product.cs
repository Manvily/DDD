using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        public NameValue Name { get; }
        public Price Price { get; }
        public Category Category { get; }
        public IEnumerable<Order> Orders { get; }

        public Product(NameValue name, Price price, Category category)
        {
            Name = name;
            Price = price;
            Category = category;
        }

        public Product()
        {
        }
    }
}
