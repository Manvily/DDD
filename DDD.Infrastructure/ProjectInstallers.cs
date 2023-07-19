using DDD.Application.Abstractions;
using DDD.Infrastructure.Commands;
using DDD.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DDD.Infrastructure
{
    public static partial class ProjectInstallers
    {
        public static IServiceCollection AddSqlServerConnection(this IServiceCollection services, IConfiguration configuration)
        {
            // DB
            var connectionString = configuration.GetConnectionString("SqlServer");
            services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(connectionString));
            
            // DI
            services.AddTransient<ICustomersCommandRepository, CustomersCommandRepository>();
            services.AddTransient<IOrderCommandRepository, OrderCommandRepository>();
            services.AddTransient<IProductsCommandRepository, ProductsCommandRepository>();

            return services;
        }
    }
}
