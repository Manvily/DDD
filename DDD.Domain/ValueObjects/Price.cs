using DDD.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class Price : ValueObject
    {
        public decimal Value { get; set; }

        public Price(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Price cannot be negative");

            Value = value;
        }

        public static implicit operator decimal(Price price)
        {
            return price.Value;
        }

        public static implicit operator Price(decimal value)
        {
            return new Price(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
