using DDD.Application.Commands.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Api.Commands.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersCommandController : Controller
    {
        private readonly ILogger<OrdersCommandController> _logger;
        private readonly IMediator _mediator;

        public OrdersCommandController(IMediator mediator, ILogger<OrdersCommandController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderCreateCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
