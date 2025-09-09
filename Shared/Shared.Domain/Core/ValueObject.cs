using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Shared.Domain.Core
{
    public abstract class ValueObject
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propsCache = new();
        
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, right) || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        protected virtual IEnumerable<object> GetEqualityComponents()
        {
            var type = GetType();

            var props = _propsCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                    .OrderBy(p => p.Name)
                    .ToArray());

            foreach (var prop in props)
            {
                var value = prop.GetValue(this);

                // Traktuj stringi jak skalar, kolekcje rozwijaj
                if (value is IEnumerable enumerable && value is not string)
                {
                    foreach (var item in enumerable)
                        yield return item!;
                }
                else
                {
                    yield return value!;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;

            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }
    }
}
