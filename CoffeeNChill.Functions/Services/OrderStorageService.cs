using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services
{
    // Handles saving and retrieving order information in Azure Table Storage.
    public class OrderStorageService
    {
        private readonly TableClient _tableClient;

        // Connects to the Orders table using the storage connection string.
        public OrderStorageService()
        {
            string connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage connection string is missing.");

            _tableClient = new TableClient(connectionString, "Orders");
        }

        // Saves an order record in the Orders table.
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

            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.UpsertEntityAsync(
                orderEntity,
                TableUpdateMode.Merge);
        }
    }
}
