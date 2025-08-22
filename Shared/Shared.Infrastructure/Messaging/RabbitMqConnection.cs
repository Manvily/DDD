using RabbitMQ.Client;

namespace Shared.Infrastructure.Messaging;

/*
 * This class is responsible for creating a RabbitMQ connection.
 * We don't want to create new connection every reqeust so here is singleton approach
 */
public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;

    public RabbitMqConnection(RabbitMQConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration.HostName,
            Port = configuration.Port,
            UserName = configuration.UserName,
            Password = configuration.Password,
            VirtualHost = configuration.VirtualHost
        };

        _connection = factory.CreateConnection();
    }

    public IModel CreateChannel() => _connection.CreateModel();

    public void Dispose() => _connection?.Dispose();
}