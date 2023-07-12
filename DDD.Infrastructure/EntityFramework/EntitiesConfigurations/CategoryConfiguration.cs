using DDD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DDD.Infrastructure.EntityFramework.EntitiesConfigurations
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.OwnsOne(category => category.Name,
             navigationBuilder =>
             {
                 navigationBuilder.Property(name => name.Name).HasColumnName("Name");
             });
        }
    }
}
