using DDD.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string City { get; }
        public string ZipCode { get; }
        public string Street { get; }
        public string Country { get; }

        public Address(string city, string zipCode, string street, string country)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException("City cannot be empty", nameof(city));
            }
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                throw new ArgumentException("Zip code cannot be empty", nameof(zipCode));
            }
            if (string.IsNullOrWhiteSpace(street))
            {
                throw new ArgumentException("Street cannot be empty", nameof(street));
            }
            if (string.IsNullOrWhiteSpace(country))
            {
                throw new ArgumentException("Country cannot be empty", nameof(country));
            }

            City = city;
            ZipCode = zipCode;
            Street = street;
            Country = country;
        }

        public Address()
        {
        }
    }
}
