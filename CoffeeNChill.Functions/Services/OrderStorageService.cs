using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services
{
    // This will handle saving and retrieving order information in Azure Table Storage.
    public class OrderStorageService
    {
        private readonly TableClient _tableClient;

        // This will onnect to the Orders table using the storage connection string.
        public OrderStorageService()
        {
            string connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage connection string is missing.");

            _tableClient = new TableClient(connectionString, "Orders");
        }

        // This will save an order record in the Orders table.
        public async Task SaveOrderAsync(Order order, string status)
        {
            var orderEntity = new TableEntity(
                order.OrderTimestamp.ToString("yyyy-MM-dd"),
                order.OrderId)
            {
                ["CustomerName"] = order.CustomerName,
                ["SelectedItemSKUs"] = string.Join(",", order.SelectedItemSKUs),
                ["TotalPrice"] = order.TotalPrice,
                ["OrderTimestamp"] = order.OrderTimestamp,
                ["Status"] = status
            };

            var result = await _tableClient.CreateIfNotExistsAsync();
            Console.WriteLine($"Orders table creation result: {result?.Value?.Name ?? "Orders already exists"}");
            Console.WriteLine("Orders table creation check completed.");
            var response = await _tableClient.UpsertEntityAsync(
            orderEntity,
            TableUpdateMode.Merge);

            Console.WriteLine($"Orders table write status: {response.Status}");
            Console.WriteLine($"Saved order {order.OrderId} with status {status}.");
        }
    }
}
