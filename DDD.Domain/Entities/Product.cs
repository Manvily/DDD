using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        public NameValue Name { get; set; }
        public Price Price { get; set; }
        public Category Category { get; set; }
        public IEnumerable<Order> Orders { get; set; }

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
