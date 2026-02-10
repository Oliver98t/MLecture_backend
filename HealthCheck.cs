using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HealthCheck
{
    private readonly ILogger<HealthCheck> _logger;

    public HealthCheck(ILogger<HealthCheck> logger)
    {
        _logger = logger;
    }

    [Function("HealthCheck")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Admin, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("API is running");
        return new OkObjectResult("API is running\n");
    }
}