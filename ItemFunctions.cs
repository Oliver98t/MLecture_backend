using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System.Text.Json;

namespace Company.Function;

public class Item
{
    public string? Type {get; set;}
    public string? Name {get; set;}
}

public class ItemFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly TableServiceClient _tableServiceClient;
    private readonly string TableName = "Items";

    public ItemFunctions(ILogger<UserFunctions> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function("GetItems")]
    public async Task<IActionResult> GetItems(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items")] HttpRequest req)
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
                    Name = item.GetString("Name"),
                    Timestamp = item.GetDateTime("Timestamp")
                }
            );
        }
        return new JsonResult(results);
    }

    [Function("GetItemsPartition")]
    public async Task<IActionResult> GetItemsPartition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items/{PartitionKey}")] HttpRequest req,
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
                    Name = item.GetString("Name"),
                    Timestamp = item.GetDateTime("Timestamp")
                }
            );
        }
        return new JsonResult(results);
    }

    [Function("CreateItem")]
    public async Task<IActionResult> CreateItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "item")] HttpRequest req)
    {
        _logger.LogInformation("Creating new Item");

        try
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // In a real app, you'd deserialize and validate the JSON
            var newItem = JsonSerializer.Deserialize<Item>(requestBody);
            if(newItem != null)
            {
                _logger.LogInformation($"{newItem.Type} {newItem.Name}");

            }
            else
            {
                throw new Exception("invalid json data");
            }

            // Get or create table
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            // Add entity
            var entity = new TableEntity(newItem.Type, Guid.NewGuid().ToString())
            {
                { "Name", newItem.Name },
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