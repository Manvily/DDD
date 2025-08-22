using Analytics.Application.Abstractions;
using MediatR;

namespace Analytics.Application.Queries;

public record GetCustomerEventsQuery(DateTime From, DateTime To) : IRequest<IEnumerable<CustomerEventData>>;
