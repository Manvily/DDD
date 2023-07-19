using DDD.Application.Queries.Customers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Api.Query.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersQueryController : ControllerBase
    {
        private readonly ILogger<CustomersQueryController> _logger;
        private readonly IMediator _mediator;

        public CustomersQueryController(IMediator mediator, ILogger<CustomersQueryController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerViewModel>>> GetCustomersList()
        {
            var result = await _mediator.Send(new CustomersAllQuery());
            return Ok(result);
        }
    }
}
