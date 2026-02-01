using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HeartBeat
{
    private readonly ILogger<HeartBeat> _logger;

    public HeartBeat(ILogger<HeartBeat> logger)
    {
        _logger = logger;
    }

    [Function("HeartBeat")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("API is running");
        return new OkObjectResult("API is running");
    }
}