using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application
{
    public static class ProjectInstallers
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddAutoMapper(assembly);

            return services;
        }
    }
}
