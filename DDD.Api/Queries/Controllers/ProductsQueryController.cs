using DDD.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.Api.Queries.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsQueryController : ControllerBase
{
    private readonly ILogger<ProductsQueryController> _logger;
    private readonly IMediator _mediator;
    
    public ProductsQueryController(IMediator mediator, ILogger<ProductsQueryController> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetProductsList()
    {
        var result = await _mediator.Send(new ProductsAllQuery());
        return Ok(result);
    }
}