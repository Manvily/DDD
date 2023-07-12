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
            var connectionString = configuration.GetConnectionString("SqlServer");

            services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(connectionString));

            return services;
        }
    }
}
