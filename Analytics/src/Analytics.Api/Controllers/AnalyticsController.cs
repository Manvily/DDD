using Analytics.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerEvents(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        try
        {
            var query = new GetCustomerEventsQuery(from, to);
            var customerEvents = await mediator.Send(query);
            return Ok(customerEvents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
