using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs.Extensions.Tables;
using Azure.Data.Tables;
using Azure;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using Common;

namespace Company.Function;

public class GroqAPI
{
    public static async Task<string> CallGroqApiAsync(string apiKey, string prompt)
    {
        var url = "https://api.groq.com/openai/v1/chat/completions";
        var payload = new
        {
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            model = "openai/gpt-oss-120b",
            temperature = 1,
            max_completion_tokens = 8192,
            top_p = 1,
            stream = true,
            reasoning_effort = "medium",
            stop = (string?)null
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var sb = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        return "";
    }
}

public class Notes
{
    private readonly ILogger<Notes> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public Notes(ILogger<Notes> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    public class CreateNotesResponse
    {
        [QueueOutput(QueueConsts.transcriptionJob, Connection = "AzureWebJobsStorage")]
        public string[]? Messages { get; set; }
        [HttpResult]
        public HttpResponseData? HttpResponse { get; set; }
    }

    private async Task AddNotesToDB(string notes)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableConsts.notesTable);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("User", Guid.NewGuid().ToString())
        {
            { "Notes", notes },
            { "Timestamp", DateTime.UtcNow }
        };
        await tableClient.AddEntityAsync(entity);
    }

    [Function("StartCreateNotes")]
    public async Task<CreateNotesResponse> StartCreateNotes(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notes/create-notes/")]
        HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body);
        string requestBody = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(requestBody);
        var root = doc.RootElement;

        string callStatus = QueueConsts.triggerSuccessMessage;
        string url = string.Empty;
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        try
        {
            await response.WriteStringAsync(callStatus);
            if (root.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(urlProp.GetString()))
            {
                url = urlProp.GetString()!;
            }
            else
            {
                throw new Exception("The 'url' property is missing or null.");
            }
            _logger.LogInformation($"{url}");
        }
        catch(Exception e)
        {
            callStatus = QueueConsts.triggerFailMessage;
            await response.WriteStringAsync(callStatus);
            _logger.LogInformation(e.Message);
        }

        return new CreateNotesResponse()
        {
            Messages = new string[] { url },
            HttpResponse = response
        };
    }

    [Function("FormatNotesTrigger")]
    public async Task FormatNotesTrigger(
        [QueueTrigger(QueueConsts.transcriptionResults, Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        string transcription = message.MessageText;
        _logger.LogInformation(transcription);
        //string notes = await GroqAPI.CallGroqApiAsync();
        await AddNotesToDB(transcription);
    }
}