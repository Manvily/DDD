using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await _healthCheckService.CheckHealthAsync();
        
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration.TotalMilliseconds
            })
        };

        return report.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
    }

    [HttpGet("mongodb")]
    public async Task<IActionResult> GetMongoDbHealth()
    {
        var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("mongodb"));
        
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration.TotalMilliseconds
            })
        };

        return report.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
    }
}
