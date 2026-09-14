//CoffeeNChill – Part 1!

//Project Overview:

CoffeeNChill is what we call a cloud-based coffee shop management system that is developed using a programming language called C# and as well as Azure Functions.

Part 1 will focuses purely on creating HTTP-triggered Azure Functions that will then allow the CoffeeNChill system to manage menu items and also all the staff documents using Azure Storage.

For local development and testing, Azurite is what is used to perform what we call Azure Storage.

The application includes the following:

- A menu item management
- A menu item category with filtering
- Staff documents that can upload
- Staff document that can be listed
- Staff document that can download
- Automated Postman testing that is provided
- Docker containerisation



//Technologies Used:

- C#
- .NET
- Azure Functions (.NET Isolated)
- Azure Table Storage
- Azure Blob Storage
- Azurite
- Docker
- Postman
- GitHub
- Classroom 50
- Visual Studio 2026



//Project Structure:

```text
CoffeeNChill.Functions
│
├── docs
│   └── CoffeeNChill.postman_collection.json
│
├── Functions
│   ├── MenuFunction.cs
│   └── FileStorageFunction.cs
│
├── Models
│   └── MenuItem.cs
│
├── Services
│   ├── MenuStorageService.cs
│   └── AzureStorageService.cs
│
├── Dockerfile
├── Program.cs
├── host.json
├── local.settings.json
├--README.md
└──References.md

//Completed by ST10440692