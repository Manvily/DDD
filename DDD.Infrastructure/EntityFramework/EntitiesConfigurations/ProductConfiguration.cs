using DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infrastructure.EntityFramework.EntitiesConfigurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.OwnsOne(product => product.Price,
                navigationBuilder =>
                {
                    navigationBuilder.Property(price => price.Value).HasColumnName("Price");
                });

            builder.OwnsOne(product => product.Name,
                navigationBuilder =>
                {
                    navigationBuilder.Property(name => name.Name).HasColumnName("Name");
                });
        }
    }
}