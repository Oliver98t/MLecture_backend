using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs.Extensions.Tables;
using Azure.Data.Tables;
using System.Linq;
using Azure;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using Common;
using AngleSharp.Common;
using AngleSharp.Dom;
using System.Runtime.ExceptionServices;

namespace Company.Function;

public class GroqAPI
{
    public static async Task<string> CallGroqApiAsync(string input)
    {
        string? apiKey = Environment.GetEnvironmentVariable("GroqApiKey");
        string promptParams = @"written using markdown with $...$ for inline and $$ on lines above and below the math block suitable for KaTex,
                                for subscript rendering use curly braces for multi-character subscripts (e.g., v_{x}),
                                do not inlcude ''' or ` in the output string
                                (give the raw string)";
        string prompt = $"Transform the following into notes under the conditons:\n{promptParams}\nTranscript:{input}";
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
        string result;
        if(GlobalConsts.GroqAPIEnable == "true")
        {
            using var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var sb = new StringBuilder();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
                var json = line.Substring(6);
                if (json == "[DONE]") break;
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("choices", out var choices))
                    {
                        foreach (var choice in choices.EnumerateArray())
                        {
                            if (choice.TryGetProperty("delta", out var delta))
                            {
                                if (delta.TryGetProperty("content", out var contentValue))
                                {
                                    sb.Append(contentValue.GetString());
                                }
                            }
                        }
                    }
                }
                catch { /* ignore malformed lines */ }
            }
            result = sb.ToString();
        }
        else
        {
            result = @"Projectile Motion – Key Concepts & Worked Example (Bob’s “tombstoning”) – General Approach: Separate motion into vertical (affected by gravity, ay = -g = -9.8 m s⁻²) and horizontal (no net force, ax = 0, speed constant); time t links both components, find t from vertical then use in horizontal. Vertical Motion – Flight Time: sy = +50 m, uy = 0 m s⁻¹, ay = +9.8 m s⁻², t = sqrt(2sy/ay) ≈ 3.19 s. Horizontal Motion – Distance D: ux = 8 m s⁻¹, ax = 0, t = 3.19 s, D = ux t ≈ 25.6 m. Final Velocity at Impact: vy = sqrt(2gs_y) ≈ 31.3 m s⁻¹ (down), vx = 8 m s⁻¹, resultant v = sqrt(vx² + vy²) ≈ 32.3 m s⁻¹, impact angle θ = tan⁻¹(vy/vx) ≈ 75.7°. Common Pitfalls: Initial vertical velocity is zero for horizontal launches; s = ut + ½at² simplifies to s = ½at² if u = 0; time is identical for both components; air resistance ignored, ax = 0. Fired Projectile Example: u = 80 m s⁻¹, α = 30°, g = 9.8 m s⁻²; ux = u cos α ≈ 69.3 m s⁻¹, uy = u sin α = 40 m s⁻¹; horizontal: x = ux t, vertical: y = uy t – ½gt², total flight time t_total = 2uy/g, range R = ux t_total. Quick Reference: Vertical displacement (rest): s = ½at²; vertical final speed: vy = sqrt(2as); horizontal distance: D = ux t; resultant speed: v = sqrt(vx² + vy²); impact angle: θ = tan⁻¹(vy/vx); vector components: vx = v cos α, vy = v sin α. Take-away: Find time from vertical, plug into horizontal for range, use components for final speed/angle; covers all standard projectile motion questions at GCSE/A-level.";
        }
        return result;
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

    private async Task AddNotesToDB(string user, string notes, string jobId)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableConsts.notesTable);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("User", Guid.NewGuid().ToString())
        {
            { "User", user },
            { "Notes", notes },
            { "JobId", jobId},
            { "Timestamp", DateTime.UtcNow }
        };
        await tableClient.AddEntityAsync(entity);
    }

    [Function("StartCreateNotes")]
    public async Task<CreateNotesResponse> StartCreateNotes(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notes/create-notes/{user}")]
        HttpRequestData req, string user)
    {
        using var reader = new StreamReader(req.Body);
        string requestBody = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(requestBody);
        var root = doc.RootElement;

        string queueData;
        string url = string.Empty;
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        try
        {
            if (root.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(urlProp.GetString()))
            {
                url = urlProp.GetString()!;
            }
            else
            {
                throw new Exception("The 'url' property is missing or null.");
            }
            _logger.LogInformation($"{url}");

            var result = new StartCreateNotesQueueData(user, url, Guid.NewGuid().ToString());
            string json = JsonSerializer.Serialize(result);
            queueData = json;
            await response.WriteStringAsync($"{json}\n");
        }
        catch(Exception e)
        {
            response = req.CreateResponse(HttpStatusCode.BadRequest);

            queueData = QueueConsts.triggerFailMessage;
            _logger.LogInformation(e.Message);
        }

        return new CreateNotesResponse()
        {
            Messages = new string[] { queueData },
            HttpResponse = response
        };
    }

    [Function("FormatNotesTrigger")]
    public async Task FormatNotesTrigger(
        [QueueTrigger(QueueConsts.transcriptionResults, Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        TranscribeTriggerQueueData? inputQueueData = JsonSerializer.Deserialize<TranscribeTriggerQueueData>(message.MessageText);
        if(inputQueueData != null)
        {
            string notes = await GroqAPI.CallGroqApiAsync(inputQueueData.Transcript);
            _logger.LogInformation(notes);
            await AddNotesToDB(inputQueueData.User, notes, inputQueueData.JobId);
        }
    }

    [Function("GetNotes")]
    public async Task<HttpResponseData> GetNotes(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes/get-notes/{user}/{jobId}")]
        HttpRequestData req, string user, string jobId)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        // CORS headers
        response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5173");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        var tableClient = _tableServiceClient.GetTableClient(TableConsts.notesTable);
        string filter = $"User eq '{user}' and JobId eq '{jobId}'";
        var result = tableClient.QueryAsync<TableEntity>(filter);
        TableEntity? first = null;
        await foreach(var entity in result)
        {
            first = entity;
            break;
        }
        string json = JsonSerializer.Serialize(
            new
                {
                    User = first?.GetString("User"),
                    Notes = first?.GetString("Notes")
                });
        await response.WriteStringAsync($"{json}\n");
        return response;
    }
}