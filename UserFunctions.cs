using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System.Text.Json;

namespace Company.Function;

public class User
{
    public string? Role { get; set; }
    public string? Email { get; set; }
}

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly TableServiceClient _tableServiceClient;
    static readonly string TableName = "Users";

    public UserFunctions(ILogger<UserFunctions> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    public static async Task<TableEntity?> checkUserExists(
        TableServiceClient tableServiceClient,
        string email)
    {
        var tableClient = tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();
        TableEntity? result;
        try
        {
            var queryResults = tableClient.QueryAsync<TableEntity>(e => e.RowKey == email);
            await foreach (var entity in queryResults)
            {
                result = entity; // Return the first matching user
            }
            result = null; // No user found
        }
        catch (Exception)
        {
            result = null; // Error occurred
        }
        return result;
    }

    public static async Task createUser(
        TableServiceClient tableServiceClient,
        string email,
        string role)
    {
        // Get or create table
        var tableClient = tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync();

        // Add entity
        var entity = new TableEntity(role, email)
            {
                { "Timestamp", DateTime.UtcNow }
            };
        await tableClient.AddEntityAsync(entity);
    }

    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "users")] HttpRequest req)
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
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "users/{PartitionKey}")] HttpRequest req,
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
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "user")] HttpRequest req)
    {
        _logger.LogInformation("Creating new User");

        try
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newUser = JsonSerializer.Deserialize<User>(requestBody);
            if (newUser != null)
            {
                _logger.LogInformation($"{newUser.Email}");

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
                { "Timestamp", DateTime.UtcNow }
            };
            await tableClient.AddEntityAsync(entity);

            return new JsonResult(entity);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new JsonResult(
                new
                {
                    Exists = true
                }
            );
        }
    }
}