using DDD.Application.Abstractions;
using DDD.Application.Cache;
using DDD.Infrastructure.Cache;
using DDD.Infrastructure.Commands;
using DDD.Infrastructure.EntityFramework;
using DDD.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Startup;

namespace DDD.Infrastructure
{
    public static partial class ProjectInstallers
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DB
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            services.AddDbContext<SqlServerContext>(options => options.UseNpgsql(connectionString));
            
            // DI
            // Commands
            services.AddTransient<ICustomersCommandRepository, CustomersCommandRepository>();
            services.AddTransient<IOrderCommandRepository, OrderCommandRepository>();
            services.AddTransient<IProductsCommandRepository, ProductsCommandRepository>();

            // Queries
            services.AddTransient<ICustomersQueryRepository, CustomersQueryRepository>();
            services.AddTransient<IProductsQueryRepository, ProductsQueryRepository>();
            services.AddTransient<IOrdersQueryRepository, OrdersQueryRepository>();
            
            // Others
            services.AddTransient<IRedisCache, RedisCache>();
            
            // RabbitMq
            services.AddHostedService<RabbitMqTopologyInitializer>();
            services.AddRabbitMQMessaging(configuration);

            return services;
        }

        public static async Task ApplyDatabaseMigrationsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SqlServerContext>();
            
            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying database migrations: {ex.Message}");
                // Don't throw here to allow the application to start even if migrations fail
            }
        }
    }
}
