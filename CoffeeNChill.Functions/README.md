# CoffeeNChill 2

## Project Overview

CoffeeNChill is a C# Azure Functions application developed for the Cloud Development 2B assignment. The application allows orders to be submitted through an HTTP endpoint and added to an Azure Storage Queue for processing.

The project uses Docker Compose to run the Azure Functions application together with Azurite, which provides local Azure Storage services for development and testing.

## Technologies Used

- C#
- .NET isolated Azure Functions
- Azure Queue Storage
- Azure Table Storage
- Azurite
- Docker and Docker Compose
- Postman
- GitHub

## Main Functionality

### Queueing Orders

The `QueueOrder` function accepts an HTTP POST request at:

`http://localhost:8080/api/orders/queue`

A valid order is added to the order-processing queue. The endpoint returns **202 Accepted** when the order is accepted for processing.

### Processing Orders

The `ProcessOrderQueue` function runs when a message is received from the `order-processing-queue`.

The function processes the order through the following statuses:

1. Received
2. Preparing
3. Ready
4. Collected

The processing results are recorded in the application logs.

### Order Storage

The `OrderStorageService` class is responsible for saving order information to the `Orders` Azure Table Storage table. The stored information includes the order ID, customer name, selected item SKUs, total price, timestamp and status.

## Running the Application

The application is configured to run using Docker Compose with two services:

- **Functions:** runs the CoffeeNChill Azure Functions application.
- **Azurite:** provides local Blob, Queue and Table Storage services.

To start the services, open a terminal in the directory containing `docker-compose.yml` and run:

```bash
docker compose up -d
```

To view the running containers:

```bash
docker ps
```

To view the Functions logs:

```bash
docker logs --tail 100 coffeenchill-functions
```

## Testing with Postman

1. Open Postman.
2. Create a new **POST** request.
3. Enter the URL:

   `http://localhost:8080/api/orders/queue`

4. Select **Body → raw → JSON**.
5. Enter a valid order, for example:

```json
{
  "OrderId": "ORD-001",
  "CustomerName": "Jane Smith",
  "SelectedItemSKUs": ["COF-001", "PAS-104"],
  "TotalPrice": 65.00,
  "OrderTimestamp": "2026-10-12T10:15:00Z"
}
```

6. Send the request.
7. Check the response and the Functions logs to confirm that the order has been queued and processed.

## Docker Image

The Functions application image used in the Docker Compose configuration is:

`paigedebeer692/coffeenchill-functions:v2.3`

## Version Control

GitHub is used for version control and to track project development through commits. The repository also contains the project files and configuration.

## Limitations and Verification

The order endpoint has returned `202 Accepted`, and the Functions logs have shown successful processing through all four order statuses. The contents of the `Orders` table should also be checked in Azure Storage Explorer to confirm that the records are visible.

## Use of AI

ChatGPT was conducted for assistance for explaining programming
concepts, troubleshooting errors, and improving my overall documentation where needed. The
project code and changes should be reviewed and understood by the
student which is me, will remain responsible for the final implementation and
submission of part 1 and 2 together.
