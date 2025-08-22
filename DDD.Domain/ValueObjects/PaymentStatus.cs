using Shared.Domain.Core;

namespace DDD.Domain.ValueObjects
{
    public class PaymentStatus : ValueObject
    {
        public bool IsPaid { get; set; }

        public PaymentStatus(bool isPaid)
        {
            IsPaid = isPaid;
        }

        public PaymentStatus()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsPaid;
        }
    }
}
