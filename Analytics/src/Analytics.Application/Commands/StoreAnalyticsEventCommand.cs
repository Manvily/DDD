using MediatR;
using Shared.Domain.Core;

namespace Analytics.Application.Commands;

public record StoreAnalyticsEventCommand(IDomainEvent DomainEvent) : IRequest<Unit>;
