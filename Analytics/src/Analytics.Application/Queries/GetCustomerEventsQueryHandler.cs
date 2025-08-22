using Analytics.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Queries;

public class GetCustomerEventsQueryHandler : IRequestHandler<GetCustomerEventsQuery, IEnumerable<CustomerEventData>>
{
    private readonly IAnalyticsEventQueryRepository _queryRepository;
    private readonly ILogger<GetCustomerEventsQueryHandler> _logger;

    public GetCustomerEventsQueryHandler(
        IAnalyticsEventQueryRepository queryRepository,
        ILogger<GetCustomerEventsQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerEventData>> Handle(GetCustomerEventsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customer events from {From} to {To}", request.From, request.To);

        var customerEvents = await _queryRepository.GetCustomerEventsAsync(request.From, request.To);
        
        _logger.LogInformation("Retrieved {Count} customer events from {From} to {To}", 
            customerEvents.Count(), request.From, request.To);

        return customerEvents;
    }
}
