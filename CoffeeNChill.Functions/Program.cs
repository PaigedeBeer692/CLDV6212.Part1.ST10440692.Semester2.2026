using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoffeeNChill.Functions.Services;

// This will then create the builder used to then also configure the Azure Functions application.
var builder = FunctionsApplication.CreateBuilder(args);

// This will then configure the application to e able to then use the HTTP Functions programming model.
builder.ConfigureFunctionsWebApplication();

// This will then register the services that are then required by the application.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// This will then register the Azure Blob Storage service for dependency injection.
builder.Services.AddSingleton<AzureStorageService>();

// This will then register the Azure Table Storage service for dependency injection.
builder.Services.AddSingleton<MenuStorageService>();

// This will then build the application and then also start the Azure Functions host.
builder.Build().Run();

//Completed by ST10440692