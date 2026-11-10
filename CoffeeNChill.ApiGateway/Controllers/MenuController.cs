using Microsoft.AspNetCore.Mvc;
using CoffeeNChill.ApiGateway.Models;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeNChill.ApiGateway.Controllers
{
    // This controller will handle menu requests sent to the API Gateway.
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // The HttpClient will then allow the Gateway to communicate with Azure Functions.
        public MenuController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: api/Menu
        // This will retrieve all menu items.
        [HttpGet]
        public async Task<IActionResult> GetAllMenuItems()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("http://localhost:8080/api/menu");

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        // GET: api/Menu/category/Coffee
        // This will retrieve menu items from a selected category.
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetMenuItemsByCategory(string category)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"http://localhost:8080/api/menu/category/{Uri.EscapeDataString(category)}");

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        // POST: api/Menu
        // This will add a new menu item.
        [Authorize(Roles = "Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(
            [FromBody] MenuItem menuItem)
        {
            string jsonContent = JsonSerializer.Serialize(menuItem);

            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(
                "http://localhost:8080/api/menu",
                content);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        // PUT: api/Menu/Coffee/COF001
        // This will update an existing menu item.
        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("{category}/{id}")]
        public async Task<IActionResult> UpdateMenuItem(
            string category,
            string id,
            [FromBody] MenuItem menuItem)
        {
            string jsonContent = JsonSerializer.Serialize(menuItem);

            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(
                $"http://localhost:8080/api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}",
                content);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        // DELETE: api/Menu/Coffee/COF001
        // This will delete a menu item.
        [Authorize(Roles = "Staff,Admin")]
        [HttpDelete("{category}/{id}")]
        public async Task<IActionResult> DeleteMenuItem(
            string category,
            string id)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(
                $"http://localhost:8080/api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}");

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "text/plain",
                StatusCode = (int)response.StatusCode
            };
        }
    }
}

