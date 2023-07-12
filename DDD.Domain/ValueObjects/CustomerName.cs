using DDD.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class CustomerName : ValueObject
    {
        public string First { get; }
        public string Last { get; }

        public CustomerName(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first))
            {
                throw new ArgumentException("First name cannot be empty", nameof(first));
            }
            if (string.IsNullOrWhiteSpace(last))
            {
                throw new ArgumentException("Last name cannot be empty", nameof(last));
            }

            First = first;
            Last = last;
        }

        public CustomerName()
        {
        }
    }
}
