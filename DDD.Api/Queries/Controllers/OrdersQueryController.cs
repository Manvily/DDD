using DDD.Application.Queries.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Api.Queries.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersQueryController : ControllerBase
{
    private readonly ILogger<OrdersQueryController> _logger;
    private readonly IMediator _mediator;
    
    public OrdersQueryController(IMediator mediator, ILogger<OrdersQueryController> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderViewModel>>> GetCustomerOrders([FromQuery]CustomerOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}