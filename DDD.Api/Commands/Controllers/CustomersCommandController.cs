using DDD.Application.Commands.Customers;
using DDD.Application.Queries.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Api.Commands.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersCommandController : Controller
    {
        private readonly ILogger<CustomersCommandController> _logger;
        private readonly IMediator _mediator;

        public CustomersCommandController(IMediator mediator, ILogger<CustomersCommandController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
