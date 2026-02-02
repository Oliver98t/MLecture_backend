using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace Company.Function;

public class Notes
{
    private readonly ILogger<Notes> _logger;
    private const string outputQueue = "transcription-job";
    private const string triggerSuccessMessage = "ready";
    private const string triggerFailMessage = "Not ready";

    public Notes(ILogger<Notes> logger)
    {
        _logger = logger;
    }

    public class MultiResponse
    {
        [QueueOutput(outputQueue, Connection = "AzureWebJobsStorage")]
        public string[]? Messages { get; set; }
        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    [Function("Notes")]
    public async Task<MultiResponse> StartCreateNotes(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notes/create-notes/")]
        HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body);
        string requestBody = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(requestBody);
        var root = doc.RootElement;

        string callStatus = triggerSuccessMessage;
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        try
        {
            await response.WriteStringAsync(callStatus);
            string? text = root.GetProperty("url").GetString();
            _logger.LogInformation($"{text}");
        }
        catch(Exception e)
        {
            callStatus = triggerFailMessage;
            await response.WriteStringAsync(callStatus);
            _logger.LogInformation(e.Message);
        }

        return new MultiResponse()
        {
            Messages = new string[] {callStatus},
            HttpResponse = response
        };
    }
}