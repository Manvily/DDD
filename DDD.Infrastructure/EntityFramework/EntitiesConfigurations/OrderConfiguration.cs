using DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infrastructure.EntityFramework.EntitiesConfigurations
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(order => order.Payment,
             navigationBuilder =>
             {
                 navigationBuilder.Property(payment => payment.IsPaid).HasColumnName("IsPaid");
             });
        }
    }
}