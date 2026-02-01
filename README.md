# AzureFuncExample

This project is an **Azure Function App** built with C# (.NET 8). It demonstrates serverless functions that can be deployed to Azure or run locally using Docker.

## Project Structure
- **AzureFuncExample.csproj**: Project file
- **Program.cs**: Main entry point
- **ItemFunctions.cs, UserFunctions.cs, HeartBeat.cs, QueueTrigger.cs, StorageQueue.cs**: Function implementations
- **host.json, local.settings.json**: Azure Functions configuration
- **Dockerfile, docker-compose.yml**: For local Docker development

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for Azure deployment)

## Recommended VS Code Extensions
To develop, run, and deploy this Azure Function App, install the following extensions:

- **Azure Functions** (`ms-azuretools.vscode-azurefunctions`):
  - Develop, debug, and deploy Azure Functions from VS Code.
- **Azure Account** (`ms-vscode.azure-account`):
  - Sign in to Azure and manage cloud resources.
- **Docker** (`ms-azuretools.vscode-docker`):
  - Build, run, and debug containers locally.
- **C#** (`ms-dotnettools.csharp`):
  - C# language support for VS Code.

## Running Locally


1. Start the services with Docker Compose:
   ```sh
   docker-compose up --build
   ```
2. The function app will be available at `http://localhost:7071`.
3. To stop the services:
   ```sh
   docker-compose down
   ```

### Local Settings
For local development, you need a `local.settings.json` file in the project root. This file contains app settings and connection strings used by Azure Functions when running locally.

**Example `local.settings.json`:**
```json
{
   "IsEncrypted": false,
   "Values": {
      "AzureWebJobsStorage": "UseDevelopmentStorage=true",
      "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
   }
}
```

> **Note:** Do not commit `local.settings.json` to source control as it may contain secrets.

### Using Azure Functions Core Tools
1. Install [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local):
   ```sh
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```
2. Start the function app:
   ```sh
   func start
   ```

## Deploying to Azure
1. Sign in to Azure:
   ```sh
   az login
   ```
2. Deploy using VS Code (right-click the project folder > **Deploy to Function App**), or use the Azure CLI:
   ```sh
   func azure functionapp publish <APP_NAME>
   ```

## Additional Resources
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [VS Code Azure Functions Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- [VS Code Docker Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)

---

**Happy coding!**
