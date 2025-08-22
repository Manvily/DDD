using Shared.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class Contact : ValueObject
    {
        public string Email { get; set; }
        public string Phone { get; set; }

        public Contact(string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty", nameof(email));
            }

            Email = email;
            Phone = phone;
        }

        public Contact()
        {
        }
    }
}
