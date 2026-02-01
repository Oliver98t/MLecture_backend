using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Company.Function;

public class MultiResponse
{
    [QueueOutput("testqueue",Connection = "AzureWebJobsStorage")]
    public string[]? Messages { get; set; }
    [HttpResult]
    public HttpResponseData? HttpResponse { get; set; }
}

public class StorageQueue
{
    private readonly ILogger<StorageQueue> _logger;

    public StorageQueue(ILogger<StorageQueue> logger)
    {
        _logger = logger;
    }

    [Function("HttpCreateQueueMessage")]
    public static async Task<MultiResponse> HttpCreateQueueMessage([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "QueueTrigger")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("HttpExample");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var message = "Queue message created!!!!!!!";

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(message);

        // Return a response to both HTTP trigger and storage output binding.
        return new MultiResponse()
        {
            // Write a single message.
            Messages = new string[] { message },
            HttpResponse = response
        };
    }
}