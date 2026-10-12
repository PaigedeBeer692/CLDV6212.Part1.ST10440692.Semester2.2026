using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Functions
{
    // This will process an order when a message is received from the queue.
    public class ProcessOrderQueue
    {
        private readonly OrderStorageService _orderStorageService;
        private readonly ILogger<ProcessOrderQueue> _logger;

        // This will Set up the services needed to save orders and record messages.
        public ProcessOrderQueue(
            OrderStorageService orderStorageService,
            ILogger<ProcessOrderQueue> logger)
        {
            _orderStorageService = orderStorageService;
            _logger = logger;
        }

        // This will run automatically when a message enters the order queue.
        [Function("ProcessOrderQueue")]
        public async Task Run(
            [QueueTrigger(
                "order-processing-queue",
                Connection = "AzureWebJobsStorage")] string message)
        {
            try
            {
                // This will check that the queue message is not empty.
                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new InvalidOperationException(
                        "The queue message is empty.");
                }

                // This will convert the JSON message into an Order object.
                Order? order = JsonSerializer.Deserialize<Order>(
                    message,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // This will check that the message contains the required order details.
                if (order == null ||
                    string.IsNullOrWhiteSpace(order.OrderId) ||
                    string.IsNullOrWhiteSpace(order.CustomerName) ||
                    order.SelectedItemSKUs == null ||
                    order.SelectedItemSKUs.Count == 0 ||
                    order.TotalPrice < 0)
                {
                    throw new InvalidOperationException(
                        "The queue message does not contain a valid order.");
                }

                // This will use the current UTC time if no order timestamp was supplied.
                if (order.OrderTimestamp == default)
                {
                    order.OrderTimestamp = DateTime.UtcNow;
                }

                // This will save the order as received.
                await _orderStorageService.SaveOrderAsync(order, "Received");
                _logger.LogInformation(
                    "Order {OrderId} status: Received.", order.OrderId);

                // This will update the order when preparation begins.
                await _orderStorageService.SaveOrderAsync(order, "Preparing");
                _logger.LogInformation(
                    "Order {OrderId} status: Preparing.", order.OrderId);

                // This will update the order when it is ready for collection.
                await _orderStorageService.SaveOrderAsync(order, "Ready");
                _logger.LogInformation(
                    "Order {OrderId} status: Ready.", order.OrderId);

                // This will record the final status after collection.
                await _orderStorageService.SaveOrderAsync(order, "Collected");
                _logger.LogInformation(
                    "Order {OrderId} status: Collected.", order.OrderId);
            }
            catch (JsonException ex)
            {
                // This will record invalid JSON and allow the queue retry process to run.
                _logger.LogError(
                    ex, "The order queue message contains invalid JSON.");
                throw;
            }
            catch (Exception ex)
            {
                // This will record other processing errors so they can be investigated.
                _logger.LogError(
                    ex, "Failed to process an order queue message.");
                throw;
            }
        }
    }
}

