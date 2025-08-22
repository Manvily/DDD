namespace Shared.Infrastructure.Messaging;

public sealed record ExchangeConfig(
    string Name,
    string Type,
    bool Durable = true,
    bool AutoDelete = false
);