using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Functions
{
    // Processes orders automatically when messages arrive in the queue.
    public class ProcessOrderQueue
    {
        private readonly OrderStorageService _orderStorageService;
        private readonly ILogger<ProcessOrderQueue> _logger;

        // Receives the services needed to save orders and record processing information.
        public ProcessOrderQueue(
            OrderStorageService orderStorageService,
            ILogger<ProcessOrderQueue> logger)
        {
            _orderStorageService = orderStorageService;
            _logger = logger;
        }

        // The queue trigger runs when a new order message arrives.
        [Function("ProcessOrderQueue")]
        public async Task Run(
            [QueueTrigger("order-processing-queue",
                Connection = "AzureWebJobsStorage")] string message)
        {
            try
            {
                // Convert the queue message from JSON into an Order object.
                Order? order = JsonSerializer.Deserialize<Order>(
                    message,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Stop processing if the message does not contain a valid order.
                if (order == null ||
                    string.IsNullOrWhiteSpace(order.OrderId) ||
                    string.IsNullOrWhiteSpace(order.CustomerName))
                {
                    throw new InvalidOperationException(
                        "The queue message does not contain a valid order.");
                }

                // Use the current UTC time if the order has no timestamp.
                if (order.OrderTimestamp == default)
                {
                    order.OrderTimestamp = DateTime.UtcNow;
                }

                // Record each stage of the order preparation process.
                await _orderStorageService.SaveOrderAsync(order, "Received");
                _logger.LogInformation(
                    "Order {OrderId} has been received.", order.OrderId);

                await _orderStorageService.SaveOrderAsync(order, "Preparing");
                _logger.LogInformation(
                    "Order {OrderId} is being prepared.", order.OrderId);

                await _orderStorageService.SaveOrderAsync(order, "Ready");
                _logger.LogInformation(
                    "Order {OrderId} is ready for collection.", order.OrderId);

                await _orderStorageService.SaveOrderAsync(order, "Collected");
                _logger.LogInformation(
                    "Order {OrderId} has been collected.", order.OrderId);
            }
            catch (JsonException ex)
            {
                // Log invalid JSON so the failed message can be investigated.
                _logger.LogError(
                    ex, "The order queue message contains invalid JSON.");

                throw;
            }
            catch (Exception ex)
            {
                // Log unexpected processing failures and allow the queue trigger
                // to retry the message according to the Azure Functions settings.
                _logger.LogError(
                    ex, "Failed to process an order queue message.");

                throw;
            }
        }
    }
}
