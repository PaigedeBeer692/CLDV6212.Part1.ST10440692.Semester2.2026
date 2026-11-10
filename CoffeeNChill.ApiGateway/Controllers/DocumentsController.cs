using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeNChill.ApiGateway.Controllers
{
    // This controller handles document requests through the API Gateway.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Admin")]
    public class DocumentsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // Connects the Gateway to the Azure Functions application.
        public DocumentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // POST: api/Documents/upload
        // Uploads a document to Azure Blob Storage.
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file to upload.");
            }

            using var formContent = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType ?? "application/octet-stream");

            formContent.Add(streamContent, "file", file.FileName);

            HttpResponseMessage response = await _httpClient.PostAsync(
                "http://localhost:8080/api/documents/upload",
                formContent);

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "text/plain",
                StatusCode = (int)response.StatusCode
            };
        }

        // GET: api/Documents
        // Retrieves a list of stored documents.
        [HttpGet]
        public async Task<IActionResult> ListDocuments()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                "http://localhost:8080/api/documents");

            string responseContent =
                await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                Content = responseContent,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        // GET: api/Documents/download/example.pdf
        // Downloads a document from Azure Blob Storage.
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadDocument(string fileName)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"http://localhost:8080/api/documents/download/{Uri.EscapeDataString(fileName)}");

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                return new ContentResult
                {
                    Content = errorMessage,
                    ContentType = "text/plain",
                    StatusCode = (int)response.StatusCode
                };
            }

            byte[] fileBytes =
                await response.Content.ReadAsByteArrayAsync();

            return File(
                fileBytes,
                "application/octet-stream",
                fileName);
        }
    }
}
