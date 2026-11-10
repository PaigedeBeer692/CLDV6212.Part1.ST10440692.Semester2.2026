using Azure;
using Azure.Data.Tables;
using CoffeeNChill.ApiGateway.Models;

namespace CoffeeNChill.ApiGateway.Services
{
    // This service connects to Azure Table Storage and manages user profiles.
    public class UserStorageService
    {
        private readonly TableClient _tableClient;

        // The connection string is read from the application configuration.
        public UserStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            // Connect to the CoffeeNChillUsers table.
            _tableClient = new TableClient(
                connectionString,
                "CoffeeNChillUsers");

            // Create the table if it does not already exist.
            _tableClient.CreateIfNotExists();
        }

        // Find a user using their email address.
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            string emailKey = email.Trim().ToLowerInvariant();

            try
            {
                Response<User> response =
                    await _tableClient.GetEntityAsync<User>(
                        "Users",
                        emailKey);

                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Return null if the user does not exist.
                return null;
            }
        }

        // Save a newly registered user.
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                user.PartitionKey = "Users";
                user.RowKey = user.Email.Trim().ToLowerInvariant();

                await _tableClient.AddEntityAsync(user);

                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // The email address already exists.
                return false;
            }
        }
    }
}
