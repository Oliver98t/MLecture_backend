using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Data.Tables;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n----------Env Variables----------\n\n\n\n\n\n\n\n\n\n\n\n\n");

foreach(var variable in Environment.GetEnvironmentVariables())
{
    Console.WriteLine(variable);
}

// Register TableServiceClient
builder.Services.AddSingleton(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    return new TableServiceClient(connectionString);
});

builder.Build().Run();