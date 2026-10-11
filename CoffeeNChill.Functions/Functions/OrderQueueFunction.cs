using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Functions
{
    // Accepts customer orders and places them on the Azure Storage Queue.
    public class OrderQueueFunction
    {
        private readonly ILogger<OrderQueueFunction> _logger;

        public OrderQueueFunction(ILogger<OrderQueueFunction> logger)
        {
            _logger = logger;
        }

        // Receives an order through POST /api/orders/queue.
        [Function("QueueOrder")]
        public async Task<HttpResponseData> QueueOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "orders/queue")] HttpRequestData req)
        {
            try
            {
                // Read the JSON request body and convert it into an Order object.
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Order? order = JsonSerializer.Deserialize<Order>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Check that the required order details have been provided.
                if (order == null ||
                    string.IsNullOrWhiteSpace(order.OrderId) ||
                    string.IsNullOrWhiteSpace(order.CustomerName) ||
                    order.SelectedItemSKUs == null ||
                    order.SelectedItemSKUs.Count == 0 ||
                    order.TotalPrice <= 0)
                {
                    var badRequest = req.CreateResponse(
                        System.Net.HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Please provide a valid order with an ID, customer name, selected items and a total price.");

                    return badRequest;
                }

                // Use UTC time when the request does not provide an order timestamp.
                if (order.OrderTimestamp == default)
                {
                    order.OrderTimestamp = DateTime.UtcNow;
                }

                // Connect to the queue using the configured storage connection string.
                string connectionString =
                    Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                    ?? throw new InvalidOperationException(
                        "AzureWebJobsStorage connection string is missing.");

                QueueClient queueClient = new QueueClient(
                    connectionString,
                    "order-processing-queue");

                // Create the queue if necessary before adding the order message.
                await queueClient.CreateIfNotExistsAsync();

                // Convert the order into JSON before placing it on the queue.
                string orderJson = JsonSerializer.Serialize(order);
                await queueClient.SendMessageAsync(orderJson);

                _logger.LogInformation(
                    "Order {OrderId} was added to the processing queue.",
                    order.OrderId);

                var response = req.CreateResponse(
                    System.Net.HttpStatusCode.Accepted);

                await response.WriteStringAsync(
                    $"Order {order.OrderId} was accepted for processing.");

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "The submitted order JSON was invalid.");

                var response = req.CreateResponse(
                    System.Net.HttpStatusCode.BadRequest);

                await response.WriteStringAsync("The request body must contain valid JSON.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add the order to the queue.");

                var response = req.CreateResponse(
                    System.Net.HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "The order could not be added to the queue.");

                return response;
            }
        }
    }
}
