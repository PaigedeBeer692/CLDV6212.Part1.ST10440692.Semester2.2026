using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    // This service will then be responsible for handling staff document storage while also using Azure Blob Storage.
    public class AzureStorageService
    {
        // The client will then use this to access the staff-docs Blob Storage container.
        private readonly BlobContainerClient _containerClient;

        public AzureStorageService(IConfiguration configuration)
        {
            // This will then get the Azure Storage connection string from the application configuration.
            string connectionString = configuration["AzureWebJobsStorage"]!;

            BlobServiceClient blobServiceClient =
                new BlobServiceClient(connectionString);

            // This will then connect to the staff-docs container where staff documents are then also stored.
            _containerClient =
                blobServiceClient.GetBlobContainerClient("staff-docs");

            _containerClient.CreateIfNotExists();
        }

        // This will then upload a document to the Blob Storage using the supplied file stream.
        public async Task UploadFileAsync(
            string fileName,
            Stream fileStream)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            // This will then upload the file and as well as replace an existing file with the same name if necessary.
            await blobClient.UploadAsync(
                fileStream,
                overwrite: true);
        }
        // This will then download the requested document from Blob Storage as a stream.
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            fileName = Uri.UnescapeDataString(fileName);

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            //This will then retrieve the document from Blob Storage without then also loading the entire file into memory.
            var response =
                await blobClient.DownloadStreamingAsync();

            return response.Value.Content;
        }
        // This will also then delete a document from the Blob Storage container if it then does exist.
        public async Task DeleteFileAsync(
            string fileName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
        // This will then retrieve all documents stored in the staff-docs Blob Storage container.
        public async Task<List<BlobItem>> ListFilesAsync()
        {
            List<BlobItem> files = new List<BlobItem>();

            // This will then go through each stored blob and then also add it to the list of available documents.
            await foreach (BlobItem blob in _containerClient.GetBlobsAsync())
            {
                files.Add(blob);
            }

            return files;
        }
    }
}//Completed by ST10361419 , ST10440692 , ST10443048