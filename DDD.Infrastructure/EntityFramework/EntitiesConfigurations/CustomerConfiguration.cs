using DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infrastructure.EntityFramework.ModelConfigurations
{
    internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(ci => ci.Id);

            builder.OwnsOne(customer => customer.Address,
                navigationBuilder =>
                {
                    navigationBuilder.Property(address => address.Street).HasColumnName("AddressStreet");
                    navigationBuilder.Property(address => address.City).HasColumnName("AddressCity");
                    navigationBuilder.Property(address => address.Country).HasColumnName("AddressCountry");
                    navigationBuilder.Property(address => address.ZipCode).HasColumnName("AddressZipCode");
                });

            builder.OwnsOne(customer => customer.Contact,
                navigationBuilder =>
                {
                    navigationBuilder.Property(contact => contact.Email).HasColumnName("Email");
                    navigationBuilder.Property(contact => contact.Phone).HasColumnName("Phone");
                });

            builder.OwnsOne(customer => customer.Name,
                navigationBuilder =>
                {
                    navigationBuilder.Property(name => name.First).HasColumnName("FirstName");
                    navigationBuilder.Property(name => name.Last).HasColumnName("LastName");
                });
        }
    }
}
