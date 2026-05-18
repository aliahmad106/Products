using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Infrastructure.Data;

namespace Products.Api.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ProductsDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ProductsDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var isDbHealthy = await CheckDatabaseConnectivity();

        if (!isDbHealthy)
        {
            _logger.LogWarning("Health check failed: database unreachable.");
            return StatusCode(503, new
            {
                status = "degraded",
                timestamp = DateTime.UtcNow,
                database = "unreachable"
            });
        }

        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            database = "connected"
        });
    }

    private async Task<bool> CheckDatabaseConnectivity()
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
