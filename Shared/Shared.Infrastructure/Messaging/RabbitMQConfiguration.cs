namespace Shared.Infrastructure.Messaging
{
    public class RabbitMQConfiguration
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        // public string ExchangeName { get; set; }
        // public string AnalyticsQueueName { get; set; } = "analytics_events";
        // public string AnalyticsRoutingKey { get; set; } = "analytics.*";
        public bool EnableRetry { get; set; } = true;
        public int RetryCount { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
        public bool RequeueOnError { get; set; } = true;
        public bool DeadLetterExchange { get; set; } = true;
    }
}