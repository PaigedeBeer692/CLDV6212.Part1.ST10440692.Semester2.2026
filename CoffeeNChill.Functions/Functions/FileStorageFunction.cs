using CoffeeNChill.Functions.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Functions
{
    // This class contains the HTTP-triggered functions used to upload, download and list staff documents.
    public class FileStorageFunction
    {
        // Storage service used to communicate with Azure Blob Storage.
        private readonly AzureStorageService _storageService;

        // The constructor receives the storage service through dependency injection.
        public FileStorageFunction(AzureStorageService storageService)
    {
        _storageService = storageService;
    }
        // Handles POST requests for uploading staff documents.
        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> UploadStaffDocument(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            // Check that the request contains a Content-Type header so the uploaded file can be processed.
            if (!req.Headers.TryGetValues("Content-Type", out var contentTypes))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide a file.");
                return badResponse;
            }

            string contentType = contentTypes.First();

            // The upload must use multipart/form-data because this request contains a file.
            if (!contentType.Contains("multipart/form-data"))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("The request must use multipart/form-data.");
                return badResponse;
            }
            // Get the multipart boundary used to separate the different sections of the request.
            string boundary = contentType.Split("boundary=")[1].Trim('"');

            // MultipartReader is used to read the uploaded file from the request body.
            var reader = new MultipartReader(boundary, req.Body);

            MultipartSection? section = await reader.ReadNextSectionAsync();

            // Process each section of the multipart request until the uploaded file is found.
            while (section != null)
            {
                // Read the Content-Disposition information to identify the uploaded file and its name.
                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    string? fileName = contentDisposition.FileNameStar?.ToString();

                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = contentDisposition.FileName?.ToString();
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.GetFileName(fileName);

                        // Upload the file stream to the staff-docs Blob Storage container.
                        await _storageService.UploadFileAsync(fileName, section.Body);

                        // Return a successful response after the file has been uploaded.
                        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                        await response.WriteStringAsync(
                            $"File '{fileName}' uploaded successfully.");

                        return response;
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            // Return 400 Bad Request when the request does not contain a file.
            var noFileResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await noFileResponse.WriteStringAsync("No file was found in the request.");

            return noFileResponse;
        }
        // Handles GET requests used to download a staff document from Blob Storage.
        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> DownloadStaffDocument(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")]
        HttpRequestData req,
        string fileName)
        {
            // Decode the file name so names containing spaces or special characters can be handled correctly.
            fileName = Uri.UnescapeDataString(fileName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide a file name.");
                return badResponse;
            }

            try
            {
                // Retrieve the requested file as a stream from Blob Storage.
                Stream fileStream = await _storageService.DownloadFileAsync(fileName);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

                // Set the response type so the downloaded content is treated as a file.
                response.Headers.Add("Content-Type", "application/octet-stream");
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                // Copy the downloaded file stream into the HTTP response sent back to the client.
                await fileStream.CopyToAsync(response.Body);

                return response;
            }
            // Handle the case where the requested document does not exist in Blob Storage.
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("The requested file was not found.");
                return notFoundResponse;
            }
            // Handle any unexpected errors that occur while downloading the document.
            catch (Exception ex)
            {
               
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while downloading the file.");
                return errorResponse;
            }
        }
        // Handles GET requests used to retrieve a list of all stored staff documents.
        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> ListStaffDocuments(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            // Retrieve the stored documents and their metadata from Blob Storage.
            var files = await _storageService.ListFilesAsync();

            // Select the information that should be returned for each document.
            var fileList = files.Select(file => new
            {
                FileName = file.Name,
                FileSize = file.Properties.ContentLength,
                LastModified = file.Properties.LastModified
            });

            // Create a successful response containing the document list.
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

            // Return the document names, sizes and last modified dates as JSON.
            await response.WriteAsJsonAsync(fileList);

            return response;
        }
    }
} //Completed by ST10440692 , ST10443048 , ST10361419