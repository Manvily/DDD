using DDD.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class NameValue : ValueObject
    {
        public string Name { get; set; }

        public NameValue(string name)
        {
            Name = name;
        }

        public NameValue()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
