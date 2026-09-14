using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    public class MenuStorageService
    {
        // This client is used to connect to and work with Azure Table Storage.
        private readonly TableClient _tableClient;

        // The constructor gets the storage connection settings and creates the MenuItems table.
        public MenuStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureWebJobsStorage"];

            _tableClient = new TableClient(
                connectionString,
                "MenuItems");

            _tableClient.CreateIfNotExists();
        }

        // Adds a new menu item to the MenuItems table.
        public async Task AddMenuItemAsync(MenuItem item)
        {
            TableEntity entity = new TableEntity(
                item.Category,
                item.Id)
            {
                ["Name"] = item.Name,
                ["Description"] = item.Description,
                ["Price"] = item.Price,
                ["IsAvailable"] = item.IsAvailable
            };

            await _tableClient.AddEntityAsync(entity);
        }

        // Retrieves all menu items stored in the MenuItems table.
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            List<MenuItem> items = new List<MenuItem>();

            await foreach (TableEntity entity in _tableClient.QueryAsync<TableEntity>())
            {
                items.Add(ConvertToMenuItem(entity));
            }

            return items;
        }

        // Retrieves menu items by using the category as the partition key.
        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(
            string category)
        {
            List<MenuItem> items = new List<MenuItem>();

            await foreach (TableEntity entity in _tableClient.QueryAsync<TableEntity>(
                filter: $"PartitionKey eq '{category}'"))
            {
                items.Add(ConvertToMenuItem(entity));
            }

            return items;
        }

        // Retrieves one menu item using its category and unique ID.
        public async Task<MenuItem?> GetMenuItemAsync(
            string category,
            string id)
        {
            try
            {
                TableEntity entity = await _tableClient.GetEntityAsync<TableEntity>(
                    category,
                    id);

                return ConvertToMenuItem(entity);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Updates an existing menu item in Azure Table Storage.
        public async Task<bool> UpdateMenuItemAsync(
            string category,
            string id,
            MenuItem item)
        {
            try
            {
                TableEntity entity = new TableEntity(category, id)
                {
                    ["Name"] = item.Name,
                    ["Description"] = item.Description,
                    ["Price"] = item.Price,
                    ["IsAvailable"] = item.IsAvailable
                };

                await _tableClient.UpdateEntityAsync(
                    entity,
                    ETag.All);

                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        // Deletes a menu item from Azure Table Storage.
        public async Task<bool> DeleteMenuItemAsync(
            string category,
            string id)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(category, id);
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        // Converts an Azure Table entity back into a MenuItem object.
        private MenuItem ConvertToMenuItem(TableEntity entity)
        {
            return new MenuItem
            {
                Category = entity.PartitionKey,
                Id = entity.RowKey,
                Name = entity.GetString("Name") ?? "",
                Description = entity.GetString("Description") ?? "",
                Price = entity.GetDouble("Price") ?? 0,
                IsAvailable = entity.GetBoolean("IsAvailable") ?? false
            };
        }
    }
} //Completed by ST10361419 , ST10443048