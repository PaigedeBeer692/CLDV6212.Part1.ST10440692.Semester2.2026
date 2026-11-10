## Part 3: CoffeeNChill API Gateway

### Project Overview

Part 3 which extends on the CoffeeNChill application by introducing an ASP.NET Core Web API Gateway. The gateway provides a central entry point for customer authentication, menu access, order placement and document access.

The project is developed in C# using ASP.NET Core and .NET 10.

### Technologies Used

- C# and .NET 10
- ASP.NET Core Web API
- Swagger / OpenAPI
- JSON Web Tokens (JWT)
- Password hashing
- Role-based authorization
- Docker
- Postman
- Git and GitHub

### Main Features

**Customer Registration and Authentication**

Customers can register an account. Password hashing is implemented to avoid storing passwords as plain text. Login authenticates users and generates a JWT for authenticated requests.

**Role-Based Menu Access**

Authorization rules restrict menu functionality according to the user's assigned role.

**Authenticated Order Placement**

Order placement requires authentication. Requests must include a valid JWT where required by the API's authorization rules.

**Document Access Control**

Document endpoints apply role-based restrictions to protect access to staff documents.

**Swagger / OpenAPI**

Swagger provides an interactive interface for viewing and testing the API endpoints during development.

### Project Structure

The `CoffeeNChill.ApiGateway` project contains the following main components:

- `Controllers` — API endpoints for authentication, documents, menu and orders.
- `Models` — request models and user-related data models.
- `Services` — token generation and user storage functionality.
- `Program.cs` — application configuration and service registration.
- `appsettings.json` — application configuration.

### Testing

The API Gateway has been tested during development. Testing included customer registration, JWT login and token generation, role-based menu restrictions, authenticated order placement and document role restrictions.

Swagger was also checked to confirm that the API documentation interface works.

### Deployment Status

Linux VPS deployment has not been completed. The application has been developed and tested locally, but a publicly accessible VPS deployment URL is not currently available.

### Use of AI

ChatGPT was consulted to assist me with understanding programming concepts, troubleshooting errors.

