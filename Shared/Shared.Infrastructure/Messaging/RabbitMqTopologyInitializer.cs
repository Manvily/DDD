using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Infrastructure.Messaging;


// TODO move to infrastructure as code (Terraform/Helm/K8s)
public class RabbitMqTopologyInitializer(RabbitMqConnection rabbitMqConnection,
        ILogger<RabbitMqTopologyInitializer> logger)
    : IHostedService
{
    public static readonly Dictionary<string, ExchangeConfig> Exchanges = new();
    private readonly IModel _channel = rabbitMqConnection.CreateChannel();


    public Task StartAsync(CancellationToken cancellationToken)
    {
        InitExchanges();
        logger.LogInformation("All exchanges initialized");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    
    private void InitExchanges()
    {
        // List of all exchanges
        Exchanges["analytics.events"] = new ExchangeConfig("analytics.events", ExchangeType.Topic);
        
        foreach (var exchange in Exchanges.Values)
        {
            DeclareExchange(exchange);
        }
    }

    private void DeclareExchange(ExchangeConfig exchange)
    {
        _channel.ExchangeDeclare(
            exchange: exchange.Name,
            type: exchange.Type,
            durable: exchange.Durable,
            autoDelete: exchange.AutoDelete);
    }
}