# Serverless File Metadata Extractor

A serverless application that reacts to file uploads in Azure Blob Storage, reads basic blob metadata, logs it, and writes a summary text file to an output container. Includes a simple copy function to mirror uploaded files to an output location. Designed for easy local development and straightforward deployment to Azure Functions.

## Features
- Automatically triggered on blob uploads.
- Reads blob properties (e.g., content type) and logs custom metadata.
- Writes a summary text file to an output container.
- Separate function to copy uploaded files to an output container.

## How it works
Two Azure Functions using Blob triggers and bindings:
- ExtractMetaData
  - Trigger: test-samples-trigger/{name}
  - Output: test-samples-output/{name}-output.txt (a small text summary)
- CopyFile
  - Trigger: test-samples-copy/{name}
  - Output: test-samples-output/{name}-output.txt (copied content)

Both functions use the storage account defined by the app setting named metadataacccount.

## Requirements
- Azure Storage account (for triggers, outputs, and runtime storage).
- Azure Functions Core Tools v4 (for local runs).
- .NET SDK compatible with the project.
- Access to create/read containers in the configured storage account.

## Local setup

1) Create containers in your storage account (names must match the bindings):
- test-samples-trigger
- test-samples-copy
- test-samples-output

2) Create a local.settings.json file in the function project directory (do not commit this file):
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "metadataacccount": "<Your Storage Connection String>"
  }
}
```
Notes:
- AzureWebJobsStorage is required by the runtime.
- metadataacccount must point to the storage account used by your blob triggers and outputs.
- For Azurite (local storage emulator), keep UseDevelopmentStorage=true for AzureWebJobsStorage. For real storage, use the connection string.

3) Run locally
- With Core Tools:
  ```bash
  func start
  ```
- Or via your IDE’s “Run/Debug” for Azure Functions.

4) Test locally
- Upload a file to test-samples-trigger to invoke ExtractMetaData:
  - Check logs for Name, ContentType, and Metadata entries.
  - A summary file {name}-output.txt will appear in test-samples-output.
- Upload a file to test-samples-copy to invoke CopyFile:
  - A copy named {name}-output.txt will appear in test-samples-output.

## Configuration
- metadataacccount: App setting containing the connection string for the storage account used by Blob triggers/outputs.
- Container paths:
  - test-samples-trigger/{name}
  - test-samples-copy/{name}
  - test-samples-output/{name}-output.txt
You can rename containers by updating the attribute strings in the function code and ensuring the containers exist.

## Deployment
1) Create an Azure Function App with a supported runtime.
2) Set app settings:
- AzureWebJobsStorage: connection string for the function runtime storage account.
- metadataacccount: connection string for the storage account used by Blob triggers/outputs.
3) Ensure containers exist in the target storage account.
4) Publish using your preferred method (IDE publish, Azure CLI, or GitHub Actions).

CLI example (conceptual):
```bash
az functionapp config appsettings set \
  --name <YOUR_FUNCTION_APP_NAME> \
  --resource-group <YOUR_RG> \
  --settings AzureWebJobsStorage="<runtime-conn-string>" metadataacccount="<trigger-output-conn-string>"
```

## Observability
- Logs are written via ILogger and visible in the local console or in Application Insights when configured for your Function App.

## Limitations and extensions
- Current metadata extraction focuses on built-in blob properties and user-defined metadata.
- To extract rich file metadata (e.g., EXIF, document properties), integrate a parser library and extend the function logic.

## Project structure (excerpt)
```
FileMetadataExtractor/
└─ Features/
   └─ AzureFunctions/
      └─ BlobFunction.cs
```

## Security
- Do not commit secrets or connection strings.
- Use managed identities or Key Vault where possible in production.
