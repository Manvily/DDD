using Analytics.Application.Abstractions;
using Analytics.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Queries.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TestController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerEventData>>> Get()
    {
        var result = await mediator.Send(new GetCustomerEventsQuery(new DateTime(2021, 01, 01), new DateTime(2026, 02, 02)));
        
        return Ok(result);
    }
}