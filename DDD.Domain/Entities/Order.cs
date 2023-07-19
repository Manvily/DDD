﻿using DDD.Domain.Core;
using DDD.Domain.ValueObjects;

namespace DDD.Domain.Entities
{
    public class Order : Entity<Guid>
    {
        public Customer Customer { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public PaymentStatus Payment { get; set; }

        public Order(Customer customer, DateTimeOffset orderDate, IEnumerable<Product> products, PaymentStatus payment)
        {
            Customer = customer;
            OrderDate = orderDate;
            Products = products;
            Payment = payment;
        }

        public Order()
        {
        }
    }
}
