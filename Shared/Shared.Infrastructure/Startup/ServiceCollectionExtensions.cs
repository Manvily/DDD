using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure RabbitMQ settings
        var rabbitMQConfig = new RabbitMQConfiguration();
        configuration.GetSection("RabbitMQ").Bind(rabbitMQConfig);
            
        services.AddSingleton(rabbitMQConfig);

        // Register RabbitMQ connection
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<RabbitMqTopologyInitializer>();

        // Register RabbitMQ services
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddSingleton<IEventConsumer, RabbitMqEventConsumer>();

        return services;
    }
}
