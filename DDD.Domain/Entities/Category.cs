using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Category : Entity<Guid>
    {
        public NameValue Name { get; }

        public Category(NameValue name)
        {
            Name = name;
        }

        public Category()
        {
        }
    }
}
