using DDD.Domain.Entities;
using DDD.Infrastructure.EntityFramework.EntitiesConfigurations;
using DDD.Infrastructure.EntityFramework.ModelConfigurations;
using Microsoft.EntityFrameworkCore;

namespace DDD.Infrastructure.EntityFramework
{
    internal class SqlServerContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        public SqlServerContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CustomerConfiguration());
            builder.ApplyConfiguration(new OrderConfiguration());
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new ProductConfiguration());
        }
    }
}
