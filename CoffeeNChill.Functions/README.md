# CoffeeNChill

## Project Overview

CoffeeNChill is a cloud-based application developed for the CLDV6212
Cloud Development assignment. The application uses Azure Functions and
Azure Storage to support menu management, staff document storage, and
order processing.

This repository contains the work for Part 1 and Part 2 of the
assignment.

## Technologies Used

-   C# and .NET isolated Azure Functions
-   Azure Table Storage
-   Azure Blob Storage
-   Azure Queue Storage
-   Azurite for local Azure Storage emulation
-   Docker and Docker Compose
-   Postman for HTTP endpoint testing
-   GitHub for source control

## Part 1: Menu and Document Management

Part 1 includes HTTP-triggered Azure Functions for managing menu items
and staff documents.

### Menu endpoints

  ---------------------------------------------------------------------------------
  Method                  Endpoint                          Purpose
  ----------------------- --------------------------------- -----------------------
  GET                     `/api/menu`                       Retrieve menu items

  POST                    `/api/menu`                       Create a menu item

  GET                     `/api/menu/category/{category}`   Retrieve menu items by
                                                            category

  PUT                     `/api/menu/{category}/{sku}`      Update a menu item

  DELETE                  `/api/menu/{category}/{sku}`      Delete a menu item
  ---------------------------------------------------------------------------------

Menu records are stored in Azure Table Storage in the `MenuItems` table.

### Staff document endpoints

  --------------------------------------------------------------------------------------
  Method                  Endpoint                               Purpose
  ----------------------- -------------------------------------- -----------------------
  POST                    `/api/documents/upload`                Upload a staff document

  GET                     `/api/documents`                       List staff documents

  GET                     `/api/documents/download/{fileName}`   Download a staff
                                                                 document by file name
  --------------------------------------------------------------------------------------

Staff documents are stored in Azure Blob Storage.

## Part 2: Order Queue Processing

Part 2 extends the application with an order-processing workflow using
Azure Queue Storage and Azure Table Storage.

### Order submission

-   **Method:** `POST`
-   **Endpoint:** `/api/orders/queue`
-   **Local URL:** `http://localhost:8080/api/orders/queue`

The request body is JSON. Example:

``` json
{
  "OrderId": "ORD-2026-8801",
  "CustomerName": "Jane Smith",
  "SelectedItemSKUs": ["COF-001", "PAS-104"],
  "TotalPrice": 65.00,
  "OrderTimestamp": "2026-10-12T10:15:00Z"
}
```

Use a unique `OrderId` for each test order and a current UTC timestamp
when testing. The values above are sample data.

The HTTP function serializes the order and sends it to the
`order-processing-queue` queue. The queue-triggered function
`ProcessOrderQueue` reads the message and stores order information in
the `Orders` table. The required order status lifecycle is:

`Received` → `Preparing` → `Ready` → `Collected`

Before submission, verify the complete queue workflow and confirm the
status changes are visible in the `Orders` table.

## Running Locally with Docker Compose

Docker Compose is configured in the root-level `docker-compose.yml` file
to start Azurite and the CoffeeNChill Functions container.

### Prerequisites

-   Docker Desktop installed and running
-   Access to the published Docker Hub images used in
    `docker-compose.yml`

### Start the application

Open PowerShell in the repository root (the folder containing
`CoffeeNChill.Part1.slnx` and `docker-compose.yml`) and run:

``` powershell
docker compose -f .\docker-compose.yml up -d
```

Check running containers:

``` powershell
docker ps
```

View the Functions logs:

``` powershell
docker logs --tail 100 coffeenchill-functions
```

View the Azurite logs:

``` powershell
docker logs --tail 100 coffeenchill-azurite
```

Stop the Compose application:

``` powershell
docker compose -f .\docker-compose.yml down
```

The Compose file should reference the published, version-tagged Docker
Hub Functions image required for Part 2. Confirm the image names and
tags in `docker-compose.yml` before submitting.

## Testing with Postman

The Postman collection is located at:

`CoffeeNChill.Functions/Docs/CoffeeNChill.postman_collection.json`

Import the collection into Postman and configure the local base URL as:

`http://localhost:8080`

Test the Part 1 menu and document endpoints and the Part 2 order
endpoint. For the order endpoint, send a `POST` request to
`/api/orders/queue` with a JSON body like the example above.

For the final demonstration and submission, verify and capture evidence
of: 1. Docker Compose starting both services. 2. A successful order
submission response. 3. The order message being processed by the
queue-triggered function. 4. The order record appearing in the `Orders`
table with the expected status transitions. 5. The Part 1 endpoints
continuing to work.

## Repository Structure

``` text
CoffeeNChill.Part1/
├── CoffeeNChill.Part1.slnx
├── docker-compose.yml
└── CoffeeNChill.Functions/
    ├── Docs/
    │   └── CoffeeNChill.postman_collection.json
    ├── Functions/
    │   ├── FileStorageFunction.cs
    │   ├── MenuFunction.cs
    │   ├── OrderQueueFunction.cs
    │   └── ProcessOrderQueue.cs
    ├── Models/
    │   ├── MenuItem.cs
    │   └── Order.cs
    ├── Services/
    │   ├── AzureStorageService.cs
    │   ├── MenuStorageService.cs
    │   └── OrderStorageService.cs
    ├── CoffeeNChill.Functions.csproj
    └── README.md
```

## Changelog

### Part 2

-   Added an `Order` model for submitted order details.
-   Added `OrderStorageService` to save order records to Azure Table
    Storage.
-   Added the `POST /api/orders/queue` HTTP endpoint to submit orders to
    Azure Queue Storage.
-   Added the `ProcessOrderQueue` queue-triggered function for order
    processing.
-   Added a root-level Docker Compose configuration for the Functions
    application and Azurite.

### Part 1

-   Added HTTP endpoints for menu item creation, retrieval, updating,
    and deletion.
-   Added category-based menu item retrieval.
-   Added staff document upload, listing, and download endpoints.
-   Added Docker support and a Postman collection for endpoint testing.

## Demonstration Video

YouTube demonstration: **\[Add the published YouTube video link here
before submission\]**

The demonstration should show the application starting with one Docker
Compose command, successful Postman requests, queue-triggered
processing, and order records/status changes in Azure Table Storage.

## Use of AI

ChatGPT was used as an assistance tool for explaining programming
concepts, troubleshooting errors, and improving documentation. The
project code and changes should be reviewed and understood by the
student, who remains responsible for the final implementation and
submission.
