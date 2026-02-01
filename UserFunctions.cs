using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System.Text.Json;

namespace Company.Function;

public class User
{
    public string? Name {get; set;}
    public string? Role {get; set;}
    public string? Email {get; set;}
    public string? Password {get; set;}

}

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly TableServiceClient _tableServiceClient;
    private readonly string TableName = "Users";

    public UserFunctions(ILogger<UserFunctions> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequest req)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();
        var items = tableClient.QueryAsync<TableEntity>();
        var results = new List<object>();
        await foreach (var item in items)
        {
            results.Add(
                new
                {
                    PartitionKey = item.PartitionKey,
                    RowKey = item.RowKey,
                    Password = item.GetString("Password"),
                    Name = item.GetString("Name"),
                    Timestamp = item.GetDateTime("Timestamp")
                }
            );
        }
        return new JsonResult(results);
    }

    [Function("GetUsersPartition")]
    public async Task<IActionResult> GetUsersPartition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{PartitionKey}")] HttpRequest req,
        string PartitionKey)
    {
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();
        var items = tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == PartitionKey);
        var results = new List<object>();
        await foreach (var item in items)
        {
            results.Add(
                new
                {
                    PartitionKey = item.PartitionKey,
                    RowKey = item.RowKey,
                    Password = item.GetString("Password"),
                    Name = item.GetString("Name"),
                    Timestamp = item.GetDateTime("Timestamp")
                }
            );
        }
        return new JsonResult(results);
    }

    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user")] HttpRequest req)
    {
        _logger.LogInformation("Creating new User");

        try
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // In a real app, you'd deserialize and validate the JSON
            var newUser = JsonSerializer.Deserialize<User>(requestBody);
            if(newUser != null)
            {
                _logger.LogInformation($"{newUser.Name} {newUser.Email}");

            }
            else
            {
                throw new Exception("invalid json data");
            }

            // Get or create table
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            // Add entity
            var entity = new TableEntity(newUser.Role, newUser.Email)
            {
                { "Name", newUser.Name },
                { "Password", newUser.Password },
                { "Timestamp", DateTime.UtcNow }
            };
            await tableClient.AddEntityAsync(entity);

            return new JsonResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new BadRequestObjectResult("Invalid request data");
        }
    }
}