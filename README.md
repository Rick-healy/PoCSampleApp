# Azure Functions Demo - Data Processing Flow

## 1. Introduction

This project is a demonstration of Azure Functions using different trigger types, showcasing how they can be chained together to create an overall flow of data. The functions work together to process, transform, and deliver data through various Azure services, illustrating a typical enterprise integration pattern.

The demo shows how Azure Functions can be used as building blocks for serverless workflows, with each function handling a specific task in the data processing pipeline. This approach enables flexible, scalable, and maintainable cloud-native applications.

## 2. Technology Components

### Architecture Overview

The solution consists of several Azure Functions that operate independently with different trigger types but can be integrated to form a complete data processing flow:

- **HTTP Trigger Functions**:
  - `HttpTrigger_Orchestrate_Main` - Main entry point for the orchestration process
  - `HttpTrigger_Utility_GetConfig` - Demonstrates configuration access
  - `HttpTrigger_Utility_CreateCSV` - Generates CSV data and sends to Azure Storage Queue
  - `HttpTrigger_Outbound_Email` - Processes queue messages based on correlation ID and generates output files

- **Service Bus Trigger Function**:
  - `ServiceBusQueueTrigger_Processor` - Processes messages from an Azure Service Bus queue

- **Test Function**:
  - `HttpTrigger_Test_NotificationReceiver` - Simulates a customer API for receiving notifications

### Intended Data Flow (End Goal)

The completed end-to-end flow will work as follows:

1. A message is received by the `ServiceBusQueueTrigger_Processor` from the Service Bus queue
2. After message validation, the processor hands off to the `HttpTrigger_Orchestrate_Main` function
3. `HttpTrigger_Orchestrate_Main` orchestrates the following flow:
   - Calls `HttpTrigger_Utility_GetConfig` to retrieve configuration values
   - Calls `HttpTrigger_Utility_CreateCSV` to generate CSV data with a correlation ID
   - Calls `HttpTrigger_Outbound_Email` to process messages with matching correlation ID
4. `HttpTrigger_Outbound_Email` collects the messages, creates a blob in storage, and sends a notification
5. The notification is received by `HttpTrigger_Test_NotificationReceiver` to simulate external system integration

This flow demonstrates a complete serverless event-driven architecture starting with a Service Bus trigger and progressing through function-to-function communication to accomplish a business process.

### Current Implementation Status

Currently, each function is implemented independently and can be tested separately. The integration between functions to achieve the end-to-end flow described above will be implemented in future updates.

## 3. Prerequisites

Although the functions run locally, this demo currently depends on a couple of Azure services:

### Azure Resources Required:

1. **Azure Service Bus**:
   - Create a namespace (Basic tier is sufficient)
   - Create a queue named "inbound_queue"
   - Generate a Shared Access Policy with Send/Listen permissions
   - Copy the connection string

2. **Azure Storage Account**:
   - Create a general-purpose storage account
   - Create a blob container named "outbound"
   - Create a storage queue named "outbound-csv-queue"
   - Copy the connection string

### Local Development Environment:

To run functions locally, you need:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#install-the-azure-functions-core-tools)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Azure Functions VS Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- [Azurite Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local development

For comprehensive setup instructions, follow the [official Microsoft guide for local Azure Functions development](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local).

## 4. Running the Functions

### Initial Setup:

1. Clone this repository
2. Update the `local.settings.json` file with your connection strings:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "PocSampleServiceBus": "YOUR_SERVICE_BUS_CONNECTION_STRING",
       "StorageConnectionString": "YOUR_STORAGE_ACCOUNT_CONNECTION_STRING"
     }
   }
   ```
3. Build the project: `dotnet build`

### Running Individual Functions:

#### HttpTrigger_Utility_GetConfig

This function demonstrates reading configuration values:

1. Start the function app locally
2. Send a GET request to: `http://localhost:7072/api/HttpTrigger_Utility_GetConfig`
3. Check terminal output to see the configuration values being logged

#### HttpTrigger_Utility_CreateCSV

This function generates CSV data and sends it to a storage queue:

1. Start the function app locally
2. Send a GET request with correlation ID on your URL: `http://localhost:7072/api/HttpTrigger_Utility_CreateCSV?correlationId=test123` If you dont provide a correlationId it will generate a random one.
3. In the Azure Portal, navigate to your storage account, select "Queues", then "outbound-csv-queue"
4. You should see messages containing your correlation ID and the generated data

#### HttpTrigger_Outbound_Email

This function processes queue messages by correlation ID and creates blobs:

1. First, ensure you have messages in the queue (using HttpTrigger_Utility_CreateCSV)
2. Start the function app locally
3. Send a GET request with your correlation ID: `http://localhost:7072/api/HttpTrigger_Outbound_Email?correlationId=test123`
4. In the Azure Portal:
   - Navigate to your storage account > "Containers" > "outbound"
   - You should see a new blob with your correlation ID in the name
   - The blob content should contain all queue messages with matching correlation ID
5. Check the terminal output for notification delivery confirmation

#### HttpTrigger_Test_NotificationReceiver

This function simulates receiving notifications:

1. Start the function app locally
2. Send a GET request with a test message: `http://localhost:7072/api/HttpTrigger_Test_NotificationReceiver?message=TestNotification`
3. Check the terminal output for "NOTIFICATION RECEIVED: TestNotification"

### Future End-to-End Flow Testing:

Once the integration between functions is implemented, the full flow will be testable by:

1. Sending a message to the Service Bus "inbound_queue" 
2. Watching as the ServiceBusQueueTrigger_Processor validates the message and calls HttpTrigger_Orchestrate_Main
3. Observing HttpTrigger_Orchestrate_Main coordinate the utility functions and HttpTrigger_Outbound_Email
4. Verifying blob creation and notification delivery to HttpTrigger_Test_NotificationReceiver

This integrated flow will demonstrate how multiple Azure Functions can seamlessly work together to process, transform, and deliver data through various Azure services.