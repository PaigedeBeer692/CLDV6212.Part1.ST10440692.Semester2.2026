using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeNChill.ApiGateway.Controllers
{
    // This controller will handle customer order requests.
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // The HttpClient will connect the Gateway to the existing Functions application.
        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // POST: api/Orders/queue
        // This will send a customer order to the Azure Storage Queue.
        [Authorize]
        [HttpPost("queue")]
        public async Task<IActionResult> QueueOrder(
            [FromBody] OrderRequest order)
        {
            // This will convert the order details into JSON.
            string jsonContent = JsonSerializer.Serialize(order);

            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            // This will forward the order to the existing Azure Function.
            HttpResponseMessage response = await _httpClient.PostAsync(
                "http://localhost:8080/api/orders/queue",
                content);

            // This will read the response from the Functions application.
            string responseContent =
                await response.Content.ReadAsStringAsync();

            // This will return the original response status to the client.
            return new ContentResult
            {
                Content = responseContent,
                ContentType = "text/plain",
                StatusCode = (int)response.StatusCode
            };
        }
    }

    // This class will then describe the information required to place an order.
    public class OrderRequest
    {
        public string OrderId { get; set; } = "";

        public string CustomerName { get; set; } = "";

        public List<string> SelectedItemSKUs { get; set; } = new();

        public decimal TotalPrice { get; set; }

        public DateTime OrderTimestamp { get; set; }
    }
}

