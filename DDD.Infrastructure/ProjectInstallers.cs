﻿using DDD.Application.Abstractions;
using DDD.Application.Cache;
using DDD.Infrastructure.Cache;
using DDD.Infrastructure.Commands;
using DDD.Infrastructure.EntityFramework;
using DDD.Infrastructure.Queries;
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

            return services;
        }
    }
}
