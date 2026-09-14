using System.Net;
using Azure;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


namespace CoffeeNChill.Functions.Functions
{
    // This class will contains the HTTP-triggered Functions and as well as the used to manage
    // This will also include the menu items in the CoffeeNChill application.
    public class MenuFunction
    {
        // This concists of the storage service used to then communicate with Azure Table Storage.
        private readonly MenuStorageService _menuStorageService;


        // This consist of the constructor that then receives the storage service through dependency injection.
        public MenuFunction(MenuStorageService menuStorageService)
        {
            _menuStorageService = menuStorageService;
        }

        // This manages the POST requests used to create a new menu item.
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> CreateMenuItem(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "menu")] HttpRequestData req)
        {
            // This consists of th read menu item information sent by the user as JSON.
            MenuItem? item = await req.ReadFromJsonAsync<MenuItem>();

            // This then checks that the menu item contains all the required information.
            // This consists as well with the category, ID and Name cannot be empty and the price cannot be negative.
            if (item == null ||
                string.IsNullOrWhiteSpace(item.Category) ||
                string.IsNullOrWhiteSpace(item.Id) ||
                string.IsNullOrWhiteSpace(item.Name) ||
                item.Price < 0)
            {
                // This then returns a 400 Bad Request response when the menu item information is then identifed as invalid.
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                // This will then provide the client with a message explaining why the request was rejected.
                await badResponse.WriteStringAsync(
                    "Invalid menu item. Category, Id and Name are required and Price cannot be negative.");

                return badResponse;
            }

            // This will then check for the Azure Table Storage to see if a menu item with the same
            // category and ID already do exist.
            MenuItem? existingItem =
                await _menuStorageService.GetMenuItemAsync(
                    item.Category,
                    item.Id);

            // This will then prevent a duplicate menu of items from being created by the client.
            if (existingItem != null)
            {
                // This will then return a 409 Conflict response when the menu item already then exist.
                var conflictResponse =
                    req.CreateResponse(HttpStatusCode.Conflict);

                // This will then tell the client that another menu item already uses a specific ID.
                await conflictResponse.WriteStringAsync(
                    "A menu item with this ID already exists.");

                return conflictResponse;
            }
            // This will then also save the new menu item to Azure Table Storage.
            await _menuStorageService.AddMenuItemAsync(item);

            // This will then return a 201 Created response because then that will mean a new menu item was successfully added.
            var response =
                req.CreateResponse(HttpStatusCode.Created);

            //This will then return the newly created menu item as JSON to the client.
            await response.WriteAsJsonAsync(item);

            // This will then send the successful response back to the client.
            return response;
        }

        // This will then handles GET requests used to retrieve all the menu items.
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> GetAllMenuItems(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu")] HttpRequestData req)
        {
            // This will then retrieve all menu items from the Azure Table Storage.
            List<MenuItem> items =
                await _menuStorageService.GetAllMenuItemsAsync();

            // This will then create a 200 OK response because the request was then identfied as successful.
            var response =
                req.CreateResponse(HttpStatusCode.OK);

            // This will then return the list of menu items as a JSON.
            await response.WriteAsJsonAsync(items);

            // This will then send the list of menu items back to the client.
            return response;
        }

        // This will then handle GET requests used to then retrieve menu items from a specific category.
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> GetMenuItemsByCategory(

            // This consist of the category that is supplied as part of the URL route.
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu/category/{category}")]
            HttpRequestData req,
            string category)
        {
            // This will then check that a category was provided before searching the table.
            if (string.IsNullOrWhiteSpace(category))
            {
                // This will then return 400 Bad Request if there is no category supplied.
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                // This will then tell the client if the category is required.
                await badResponse.WriteStringAsync(
                    "Category is required.");

                return badResponse;
            }

            // This will then retrieve only the menu items that belong to the requested category.
            List<MenuItem> items =
                await _menuStorageService.GetMenuItemsByCategoryAsync(
                    category);

            // This will then create a successful 200 OK response.
            var response =
                req.CreateResponse(HttpStatusCode.OK);

            // This will then return the filtered menu items as JSON.
            await response.WriteAsJsonAsync(items);

            // This will then send the category results back to the client.
            return response;
        }

        // This will then handle PUT requests used to then update an existing menu item.
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> UpdateMenuItem(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "put",
                Route = "menu/{category}/{id}")]
            HttpRequestData req,
            string category,
            string id)
        {
            // This will then read the updated menu item information from the request body.
            MenuItem? item =
                await req.ReadFromJsonAsync<MenuItem>();

            // This will then validate the updated menu item before saving the changes.
            if (item == null ||
                string.IsNullOrWhiteSpace(item.Name) ||
                item.Price < 0)
            {
                // This will then return 400 Bad Request when the updated menu item is then indetfied as invalid.
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                // This will then provide a message explaining that the menu item information is indentfied as invalid.
                await badResponse.WriteStringAsync(
                    "Invalid menu item.");

                return badResponse;
            }

            // This will then send the updated menu item to the storage service.
            // Then the category and ID will identify which existing item must then be changed.
            bool updated =
                await _menuStorageService.UpdateMenuItemAsync(
                    category,
                    id,
                    item);

            // This will then check whether the storage service was able to find and as well as update the menu item.
            if (!updated)
            {
                // This will then return 404 Not Found when the requested menu item then does not exist.
                var notFoundResponse =
                    req.CreateResponse(HttpStatusCode.NotFound);

                // This will then inform the client that the requested menu item could not be found.
                await notFoundResponse.WriteStringAsync(
                    "Menu item was not found.");

                return notFoundResponse;
            }

            // This will then make sure the response contains the category and as well as the ID from the URL.
            item.Category = category;
            item.Id = id;

            // This will then create a 200 OK response because the menu item was then updated successfully.
            var response =
                req.CreateResponse(HttpStatusCode.OK);

            // This will then return the updated menu item then as a JSON.
            await response.WriteAsJsonAsync(item);

            // This will then send the updated menu item back to the client.
            return response;
        }

        //This will then handles DELETE requests used to then remove a menu item.
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> DeleteMenuItem(

            // This category and ID are then supplied in the URL to be able identify the menu item to delete.
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
                Route = "menu/{category}/{id}")]
            HttpRequestData req,
            string category,
            string id)
        {
            // This then will ask the storage service to then delete the selected menu item from Azure Table Storage.
            bool deleted =
                await _menuStorageService.DeleteMenuItemAsync(
                    category,
                    id);

            // This will then check whether the menu item was found and successfully deleted.
            if (!deleted)
            {
                // This will then return 404 Not Found when the requested menu item then does not exist.
                var notFoundResponse =
                    req.CreateResponse(HttpStatusCode.NotFound);

                // This will then inform the client that the menu item could not be found.
                await notFoundResponse.WriteStringAsync(
                    "Menu item was not found.");

                return notFoundResponse;
            }

            // This will then create a successful 200 OK response after deleting the menu item.
            var response =
                req.CreateResponse(HttpStatusCode.OK);

            // This will then confirm to the client that the menu item was then deleted successfully.
            await response.WriteStringAsync(
                "Menu item deleted successfully.");

            // This then will send the deletion confirmation back to the client.
            return response;
        }
    }
}
//Completed by ST10440692 