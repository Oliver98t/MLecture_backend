using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace Company.Function;

public class QueueTrigger
{
    private readonly ILogger<QueueTrigger> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public QueueTrigger(ILogger<QueueTrigger> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function(nameof(QueueTrigger))]
    public async Task Run([QueueTrigger("testqueue", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);
                   // Get or create table
            var tableClient = _tableServiceClient.GetTableClient("QueueMessages");
            await tableClient.CreateIfNotExistsAsync();

            // Add entity
            var entity = new TableEntity("testqueue", Guid.NewGuid().ToString())
            {
                { "Message", message.MessageText },
                { "Timestamp", DateTime.UtcNow }
            };
            await tableClient.AddEntityAsync(entity);
    }
}